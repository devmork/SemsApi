using Azure;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SemsApi.Data;
using SemsApi.DTO.ApplicationUser;
using SemsApi.DTO.Authentication;
using SemsApi.DTO.Student;
using SemsApi.DTO.Teacher;
using SemsApi.Exceptions;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace SemsApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public AuthService(ITokenService tokenService, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<UserResponse?> GetCurrentUserAsync(int userId, CancellationToken ct = default)
        {
            var user = await _userManager.Users
                         .AsNoTracking()
                         .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user is null || user.Status != UserStatus.Active)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserResponse(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.PictureUrl,
                roles.FirstOrDefault());
        }

        public async Task<AuthTokenResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var (jwt, jwtExp) = _tokenService.GenerateAccessToken(user, roles);
            var refresh = _tokenService.GenerateRefreshToken();

            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refresh.Hash,
                ExpiresAtUtc = refresh.ExpiresAtUtc,
            });
            await _dbContext.SaveChangesAsync(ct);
            return new AuthTokenResponse(jwt, jwtExp, refresh.Token, refresh.ExpiresAtUtc);

        }

        public async Task LogoutAsync(string? refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(refreshToken)) return;
            var hash = _tokenService.Hash(refreshToken);
            var stored = await _dbContext.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAtUtc == null, ct);
            if (stored is null) return;
            stored.RevokedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<AuthTokenResponse> RefreshTokenAsync(string? refreshToken, CancellationToken ct = default)
        {
            const string invalid = "Invalid or expired refresh token";
            if (string.IsNullOrEmpty(refreshToken)) throw new RefreshTokenException("Missing refresh token");

            var hash = _tokenService.Hash(refreshToken);
            var stored = await _dbContext.RefreshTokens.Include(t => t.User)
                                .SingleOrDefaultAsync(t => t.TokenHash == hash, ct)
                                ?? throw new RefreshTokenException(invalid);

            if (stored.IsRevoked)
            {
                await _dbContext.RefreshTokens.Where(t => t.UserId == stored.UserId && t.RevokedAtUtc == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, DateTime.UtcNow));
                throw new RefreshTokenException(invalid);
            }
            if (stored.IsExpired || stored.User.Status != UserStatus.Active)
                throw new RefreshTokenException(invalid);

            var roles = await _userManager.GetRolesAsync(stored.User);
            var (jwt, jwtExp) = _tokenService.GenerateAccessToken(stored.User, roles);
            var next = _tokenService.GenerateRefreshToken();

            stored.RevokedAtUtc = DateTime.UtcNow;
            stored.ReplacedByTokenHash = next.Hash;

            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = stored.UserId,
                TokenHash = next.Hash,
                ExpiresAtUtc = next.ExpiresAtUtc,
            });
            await _dbContext.SaveChangesAsync(ct);
            return new AuthTokenResponse(jwt, jwtExp, next.Token, next.ExpiresAtUtc);
        }

        public async Task<GoogleSignInResponse> SignInWithGoogleAsync(ClaimsPrincipal? principal)
        {
            var email = principal?.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
            var googleSub = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (principal is null || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleSub))
                throw new SignInRejectedException(SignInErrorCodes.GoogleAuthFailed);

            if (!string.Equals(principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase))
                throw new SignInRejectedException(SignInErrorCodes.EmailNotVerified);

            var first = principal!.FindFirstValue(ClaimTypes.GivenName);
            var last = principal!.FindFirstValue(ClaimTypes.Surname);
            var picture = principal!.FindFirstValue("urn:google:picture");

            // Step 5: existing account? Google "sub" first, then email
            var isNew = false;
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.GoogleSubjectId == googleSub) ?? await _userManager.FindByEmailAsync(email!);


            if (user is null)
            {
                // Step 6: register
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    GoogleSubjectId = googleSub!,
                    FirstName = first ?? email!.Split('@')[0],
                    LastName = last ?? string.Empty,
                    PictureUrl = picture,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                var created = await _userManager.CreateAsync(user);
                if (!created.Succeeded)
                    throw new RegistrationFailedException(created.Errors.Select(e => e.Description));

                var role = await _userManager.AddToRoleAsync(user, "Student");
                if (!role.Succeeded)
                {
                    await _userManager.DeleteAsync(user); 
                    throw new RegistrationFailedException(role.Errors.Select(e => e.Description));
                }
                isNew = true;
            }
            else
            {
                if (string.IsNullOrEmpty(user.GoogleSubjectId))
                    user.GoogleSubjectId = googleSub;                       
                else if (user.GoogleSubjectId != googleSub)
                    throw new SignInRejectedException(SignInErrorCodes.AccountConflict);

                if (user.Status != UserStatus.Active)
                    throw new SignInRejectedException(SignInErrorCodes.AccountInactive);

                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(picture)) user.PictureUrl = picture;
                await _userManager.UpdateAsync(user);
            }

            return new GoogleSignInResponse(user, isNew);
        }
    }
}
