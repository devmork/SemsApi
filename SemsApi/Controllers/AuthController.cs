using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Services;

namespace SemsApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
    private readonly IGoogleAuthenticationService _googleAuth;
    private readonly IJwtService _jwtService;

    public AuthController(
        ApplicationDbContext context,
            UserManager<User> userManager,
        IGoogleAuthenticationService googleAuth,
        IJwtService jwtService)
    {
        _context = context;
            _userManager = userManager;
        _googleAuth = googleAuth;
        _jwtService = jwtService;
    }

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

        // 3. Check email verification
        if (!payload.EmailVerified)
        {
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                Message = "Your Google email is not verified.",
                ErrorCode = "EMAIL_NOT_VERIFIED"
            });
        }

        // 4. Check if the email domain is authorized
        var domain = payload.Email.Split('@').Last();
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

        // 5. Get or create the user (shared logic)
        var (user, errorCode) = await GetOrCreateUserAsync(payload);
        if (user == null)
        {
            return StatusCode(403, new LoginResponseDto
            {
                Success = false,
                Message = "User creation failed.",
                ErrorCode = errorCode ?? "USER_CREATION_FAILED"
            });
        }

        // 6. Update last login and save
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 7. Build response
        var response = new LoginResponseDto
        {
            Success = true,
            Message = "Login successful.",
            Token = _jwtService.GenerateToken(user),
            User = new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.Name
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

    private async Task<(User? user, string? errorCode)> GetOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .FirstOrDefaultAsync(u => u.GoogleSubjectId == payload.Subject);

        if (user != null)
            return (user, null);

        // Auto‑create new user with default role "Student"
        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole == null)
            return (null, "ROLE_NOT_FOUND");

        user = new User
        {
            GoogleSubjectId = payload.Subject,
            Email = payload.Email,
            FirstName = payload.GivenName ?? "Unknown",
            LastName = payload.FamilyName ?? "Unknown",
            RoleId = studentRole.RoleId,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .FirstOrDefaultAsync(u => u.UserId == user.UserId);

        return (user, null);
    }

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

        if (!payload.EmailVerified)
        {
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                Message = "Google email is not verified.",
                ErrorCode = "EMAIL_NOT_VERIFIED"
            });
        }

        var domain = payload.Email.Split('@').Last();
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

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .FirstOrDefaultAsync(u => u.GoogleSubjectId == payload.Subject);

        if (user is null)
        {
            return StatusCode(403, new LoginResponseDto
            {
                Success = false,
                Message = "This Google account is not registered in SEMS.",
                ErrorCode = "USER_NOT_REGISTERED"
            });
        }

        if (user.Status != "Active")
        {
            return StatusCode(403, new LoginResponseDto
            {
                Success = false,
                Message = "This account is disabled.",
                ErrorCode = "ACCOUNT_DISABLED"
            });
        }

            // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var response = new LoginResponseDto
        {
            Success = true,
            Message = "Login successful.",
            Token = _jwtService.GenerateToken(user),
            User = new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.Name
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

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Status == "Active");

        if (user is null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.Name
        });
    }
}
}