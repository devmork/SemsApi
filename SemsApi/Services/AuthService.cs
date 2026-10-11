using Azure;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SemsApi.Configuration;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace SemsApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly AuthOptions _authOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GoogleOptions _options;
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        public AuthService(
                ApplicationDbContext context,
                UserManager<User> userManager,
                RoleManager<Role> roleManager,
                IJwtService jwtService,
                IOptions<AuthOptions> authOptions,
                IOptions<GoogleOptions> options,
                IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _authOptions = authOptions.Value;
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ActionResult<UserProfileDto>> GetUserProfileAsync()
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return new UnauthorizedResult();

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value && u.Status == "Active");

            if (user is null)
                return new UnauthorizedResult();

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Unknown"
            };
        }

        public async Task<ActionResult<LoginResponseDto>> GoogleCallbackAsync(GoogleCallbackRequest request)
        {
            var tokenResponse = await ExchangeGoogleAuthCodeForTokensAsync(request.Code);
            if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.IdToken))
                return LoginFailure("Failed to exchange authorization code.",
                    "TOKEN_EXCHANGE_FAILED", StatusCodes.Status401Unauthorized);

            var payload = await ValidateGoogleIdTokenAsync(tokenResponse.IdToken);
            if (payload is null)
                return LoginFailure("Invalid ID token.",
                    "INVALID_ID_TOKEN", StatusCodes.Status401Unauthorized);

            return await ProcessGoogleLoginAsync(payload);
        }

        public async Task<ActionResult<LoginResponseDto>> GoogleLoginAsync(string idToken)
        {
            var payload = await ValidateGoogleIdTokenAsync(idToken);
            if (payload is null)
                return LoginFailure("Invalid Google token.",
                    "INVALID_GOOGLE_TOKEN", StatusCodes.Status401Unauthorized);

            return await ProcessGoogleLoginAsync(payload);
        }

        public async Task LogoutAsync()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) return;

            if (ctx.Request.Cookies.TryGetValue("refresh_token", out var raw) && !string.IsNullOrEmpty(raw))
            {
                var hash = Hash(raw);
                var stored = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null);
                if (stored is not null)
                {
                    stored.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            ctx.Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                Path = "/api/auth",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
        }

        public Task SetRefreshTokenCookieAsync(string refreshToken)
        {
            var ctx = _httpContextAccessor.HttpContext;
            ctx?.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth",
                MaxAge = TimeSpan.FromDays(7)
            });

            return Task.CompletedTask;
        }

        public async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleIdTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _options.ClientId }
                };
                return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                return null;
            }
        }

        public async Task<GoogleTokenResponse?> ExchangeGoogleAuthCodeForTokensAsync(string code)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var parameters = new List<KeyValuePair<string, string>>
        {
            new("code", code),
            new("client_id", _options.ClientId),
            new("client_secret", _options.ClientSecret),
            new("redirect_uri", _options.RedirectUri),
            new("grant_type", "authorization_code")
        };

            using var client = new HttpClient();
            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return tokenResponse;

        }

        private async Task<ActionResult<LoginResponseDto>> ProcessGoogleLoginAsync(
            GoogleJsonWebSignature.Payload payload)
        {
            if (!payload.EmailVerified)
                return LoginFailure("Google email is not verified.",
                    "EMAIL_NOT_VERIFIED", StatusCodes.Status401Unauthorized);

            var domain = payload.Email.Split('@').Last();
            var domainAuthorized = _authOptions.AuthorizedDomains
                .Any(d => string.Equals(d, domain, StringComparison.OrdinalIgnoreCase));

            if (!domainAuthorized)
                return LoginFailure("This email domain is not authorized for SEMS.",
                    "DOMAIN_NOT_AUTHORIZED", StatusCodes.Status403Forbidden);

            var (user, errorCode) = await GetOrCreateUserAsync(payload);
            if (user is null)
                return LoginFailure("Unable to process user account.",
                    errorCode ?? "USER_PROCESSING_FAILED", StatusCodes.Status403Forbidden);

            if (user.Status != "Active")
                return LoginFailure("This account is disabled.",
                    "ACCOUNT_DISABLED", StatusCodes.Status403Forbidden);

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var raw = CreateRefreshToken(user);
            await _context.SaveChangesAsync();
            await SetRefreshTokenCookieAsync(raw);

            return await BuildLoginResponseAsync(user);
        }

        private async Task<(User? user, string? errorCode)> GetOrCreateUserAsync(
           GoogleJsonWebSignature.Payload payload)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GoogleSubjectId == payload.Subject);

            if (user is not null)
                return (user, null);

            user = new User
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = payload.EmailVerified,
                GoogleSubjectId = payload.Subject,
                FirstName = payload.GivenName ?? "Unknown",
                LastName = payload.FamilyName ?? "Unknown",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return (null, "USER_CREATION_FAILED");

            if (!await _roleManager.RoleExistsAsync("Student"))
                return (null, "ROLE_NOT_FOUND");

            await _userManager.AddToRoleAsync(user, "Student");
            return (user, null);
        }

        private int? GetCurrentUserId()
        {
            var sub = _httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return int.TryParse(sub, out var id) ? id : null;
        }

        private static ActionResult<LoginResponseDto> LoginFailure(
            string message, string errorCode, int statusCode)
        {
            var dto = new LoginResponseDto
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };

            return new ObjectResult(dto) { StatusCode = statusCode };
        }


        private static string Hash(string token) =>
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // Adds a new token row to the context (does not save) and returns the raw value
        private string CreateRefreshToken(User user)
        {
            var raw = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = Hash(raw),
                ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime)
            });
            return raw;
        }

        public async Task<ActionResult<LoginResponseDto>> RefreshAsync(string refreshToken)
        {
            var hash = Hash(refreshToken);
            var stored = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored is null)
                return LoginFailure("Invalid refresh token.", "INVALID_REFRESH_TOKEN",
                    StatusCodes.Status401Unauthorized);

            // A revoked token being used again means it may have been stolen: kill the whole chain
            if (stored.RevokedAt is not null)
            {
                var active = await _context.RefreshTokens
                    .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                    .ToListAsync();
                active.ForEach(t => t.RevokedAt = DateTime.UtcNow);
                await _context.SaveChangesAsync();

                return LoginFailure("Refresh token was already used.", "REFRESH_TOKEN_REUSED",
                    StatusCodes.Status401Unauthorized);
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
                return LoginFailure("Refresh token expired.", "REFRESH_TOKEN_EXPIRED",
                    StatusCodes.Status401Unauthorized);

            var user = stored.User;
            if (user.Status != "Active")
                return LoginFailure("This account is disabled.", "ACCOUNT_DISABLED",
                    StatusCodes.Status403Forbidden);

            // Rotate: revoke old, create new, one SaveChanges
            var newRaw = CreateRefreshToken(user);
            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByTokenHash = Hash(newRaw);
            await _context.SaveChangesAsync();

            await SetRefreshTokenCookieAsync(newRaw);
            return await BuildLoginResponseAsync(user);
        }

        private async Task<LoginResponseDto> BuildLoginResponseAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            await _context.Entry(user).Reference(u => u.Student).LoadAsync();
            await _context.Entry(user).Reference(u => u.Teacher).LoadAsync();

            return new LoginResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = _jwtService.GenerateToken(user, roles),
                User = new UserProfileDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = roles.FirstOrDefault() ?? "Unknown"
                },
                Student = user.Student is null ? null : new StudentDto
                {
                    StudentId = user.Student.StudentId,
                    StudentNumber = user.Student.StudentNumber,
                    GradeLevel = user.Student.GradeLevel,
                    Section = user.Student.Section
                },
                Teacher = user.Teacher is null ? null : new TeacherDto
                {
                    TeacherId = user.Teacher.TeacherId,
                    EmployeeNumber = user.Teacher.EmployeeNumber,
                    Department = user.Teacher.Department
                }
            };
        }
    }
}
