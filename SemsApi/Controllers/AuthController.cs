using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO;
using SemsApi.Interfaces;
using SemsApi.Services;

namespace SemsApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IGoogleAuthenticationService _googleAuth;
    private readonly IJwtService _jwtService;

    public AuthController(
        ApplicationDbContext context,
        IGoogleAuthenticationService googleAuth,
        IJwtService jwtService)
    {
        _context = context;
        _googleAuth = googleAuth;
        _jwtService = jwtService;
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