using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Models;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IGoogleAuthenticationService _googleAuth;
        private readonly IJwtService _jwtService;

        public AuthController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IGoogleAuthenticationService googleAuth,
            IJwtService jwtService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _googleAuth = googleAuth;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Google OAuth Callback (Authorization Code flow)
        /// </summary>
        [HttpPost("google-callback")]
        public async Task<ActionResult<LoginResponseDto>> GoogleCallback([FromBody] GoogleCallbackRequest request)
        {
            // 1. Exchange authorization code for tokens
            var tokenResponse = await _googleAuth.ExchangeCodeForTokensAsync(request.Code);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Failed to exchange authorization code.",
                    ErrorCode = "TOKEN_EXCHANGE_FAILED"
                });
            }

            // 2. Validate the ID token
            var payload = await _googleAuth.ValidateAsync(tokenResponse.IdToken);
            if (payload == null)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid ID token.",
                    ErrorCode = "INVALID_ID_TOKEN"
                });
            }

            return await ProcessGoogleLoginAsync(payload);
        }

        /// <summary>
        /// Google Login using ID Token (common for mobile / SPA)
        /// </summary>
        [HttpPost("google")]
        public async Task<ActionResult<LoginResponseDto>> Google([FromBody] GoogleLoginRequest request)
        {
            var payload = await _googleAuth.ValidateAsync(request.IdToken);
            if (payload is null)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid Google token.",
                    ErrorCode = "INVALID_GOOGLE_TOKEN"
                });
            }

            return await ProcessGoogleLoginAsync(payload);
        }

        /// <summary>
        /// Shared logic for both Google login methods
        /// </summary>
        private async Task<ActionResult<LoginResponseDto>> ProcessGoogleLoginAsync(GoogleJsonWebSignature.Payload payload)
        {
            // 1. Email must be verified
            if (!payload.EmailVerified)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Success = false,
                    Message = "Google email is not verified.",
                    ErrorCode = "EMAIL_NOT_VERIFIED"
                });
            }

            // 2. Check authorized domain
            var domain = payload.Email.Split('@').Last().ToLower();
            var domainAuthorized = await _context.AuthorizedEmailDomains
                .AnyAsync(d => d.Domain == domain && d.IsActive);

            if (!domainAuthorized)
            {
                return StatusCode(403, new LoginResponseDto
                {
                    Success = false,
                    Message = "This email domain is not authorized for SEMS.",
                    ErrorCode = "DOMAIN_NOT_AUTHORIZED"
                });
            }

            // 3. Get or create user
            var (user, errorCode) = await GetOrCreateUserAsync(payload);
            if (user == null)
            {
                return StatusCode(403, new LoginResponseDto
                {
                    Success = false,
                    Message = "Unable to process user account.",
                    ErrorCode = errorCode ?? "USER_PROCESSING_FAILED"
                });
            }

            // 4. Check if account is active
            if (user.Status != "Active")
            {
                return StatusCode(403, new LoginResponseDto
                {
                    Success = false,
                    Message = "This account is disabled.",
                    ErrorCode = "ACCOUNT_DISABLED"
                });
            }

            // 5. Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // 6. Get roles from Identity
            var roles = await _userManager.GetRolesAsync(user);

            // 7. Load Student / Teacher profiles
            await _context.Entry(user).Reference(u => u.Student).LoadAsync();
            await _context.Entry(user).Reference(u => u.Teacher).LoadAsync();

            // 8. Build response
            var response = new LoginResponseDto
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
                Student = user.Student == null ? null : new StudentDto
                {
                    StudentId = user.Student.StudentId,
                    StudentNumber = user.Student.StudentNumber,
                    GradeLevel = user.Student.GradeLevel,
                    Section = user.Student.Section
                },
                Teacher = user.Teacher == null ? null : new TeacherDto
                {
                    TeacherId = user.Teacher.TeacherId,
                    EmployeeNumber = user.Teacher.EmployeeNumber,
                    Department = user.Teacher.Department
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Finds existing user by GoogleSubjectId or creates a new one with Student role
        /// </summary>
        private async Task<(User? user, string? errorCode)> GetOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
        {
            // Try to find existing user
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GoogleSubjectId == payload.Subject);

            if (user != null)
                return (user, null);

            // Create new user
            user = new User
            {
                UserName = payload.Email,               // Identity requires UserName
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
            {
                return (null, "USER_CREATION_FAILED");
            }

            // Assign default "Student" role
            if (await _roleManager.RoleExistsAsync("Student"))
            {
                await _userManager.AddToRoleAsync(user, "Student");
            }
            else
            {
                return (null, "ROLE_NOT_FOUND");
            }

            return (user, null);
        }

        /// <summary>
        /// Get current logged-in user profile
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> Me()
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status == "Active");

            if (user is null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Unknown"
            });
        }
    }
}