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
            var role = roles.FirstOrDefault();

            // Only fetch the academic graph if the account's primary role is "Student".
            // Admin / QA / Teacher keep the existing behavior (studentProfile = null).
            StudentProfileDto? studentProfile = null;
            if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                studentProfile = await BuildStudentProfileAsync(userId, ct);
            }

            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            return new UserResponse(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                fullName,
                user.PictureUrl,
                role,
                studentProfile);
        }

        /// <summary>
        /// Builds the consolidated student profile from the authenticated ApplicationUser.Id.
        /// Uses a single user-scoped chain (never trusts a client-supplied student id)
        /// and projects directly into DTOs (no Include chains, no N+1).
        /// </summary>
        private async Task<StudentProfileDto?> BuildStudentProfileAsync(int userId, CancellationToken ct)
        {
            // ApplicationUser.Id -> Student.UserId
            var student = await _dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);

            if (student is null)
                return null; // account exists but no academic record was linked → profile is null

            // -----------------------------------------------------------------
            // Current term enrollment  (StudentTermEnrollment is the source of truth)
            // Rule: newest ACTIVE term by (SchoolYear, Term, EnrolledAtUtc).
            // -----------------------------------------------------------------
            var currentTerm = await _dbContext.StudentTermEnrollments
                .AsNoTracking()
                .Where(t => t.StudentId == student.StudentId && t.Status == "Enrolled")
                .OrderByDescending(t => t.SchoolYear)
                .ThenByDescending(t => t.Term)
                .ThenByDescending(t => t.EnrolledAtUtc)
                .FirstOrDefaultAsync(ct);

            // -----------------------------------------------------------------
            // Subjects: only ACTIVE enrollments in ACTIVE offerings.
            // Single projection → one SQL query, no N+1, no entity graphs.
            // -----------------------------------------------------------------
            var subjectRows = await _dbContext.StudentSubjectEnrollments
                .AsNoTracking()
                .Where(e => e.StudentId == student.StudentId
                         && e.Status == "Enrolled"
                         && e.SubjectOffering.IsActive)
                .OrderBy(e => e.SubjectOffering.SchoolYear)
                .ThenBy(e => e.SubjectOffering.Term)
                .ThenBy(e => e.SubjectOffering.Subject.Code)
                .Select(e => new
                {
                    e.Status,
                    e.SubjectOfferingId,
                    e.SubjectOffering.SubjectId,
                    SubjectCode = e.SubjectOffering.Subject.Code,
                    SubjectDescription = e.SubjectOffering.Subject.Description,
                    e.SubjectOffering.GradeLevel,
                    e.SubjectOffering.Section,
                    e.SubjectOffering.SchoolYear,
                    e.SubjectOffering.Term,
                    e.SubjectOffering.TeacherId,
                    TeacherFirstName = e.SubjectOffering.Teacher != null ? e.SubjectOffering.Teacher.User.FirstName : null,
                    TeacherLastName = e.SubjectOffering.Teacher != null ? e.SubjectOffering.Teacher.User.LastName : null,
                    TeacherEmployeeNumber = e.SubjectOffering.Teacher != null ? e.SubjectOffering.Teacher.EmployeeNumber : null
                })
                .ToListAsync(ct);

            var subjects = subjectRows
                .Select(s => new StudentProfileSubjectDto(
                    s.SubjectOfferingId,
                    s.SubjectId,
                    s.SubjectCode,
                    s.SubjectDescription,
                    GradeLevelHelper.ToDisplay(s.GradeLevel),
                    s.Section,
                    s.SchoolYear,
                    s.Term,
                    s.TeacherId.HasValue
                        ? new StudentProfileTeacherDto(
                            s.TeacherId.Value,
                            s.TeacherEmployeeNumber,
                            $"{s.TeacherFirstName} {s.TeacherLastName}".Trim())
                        : null,
                    s.Status))
                .ToList();

            // -----------------------------------------------------------------
            // Profile summary. Prefer the active StudentTermEnrollment; only fall
            // back to the (potentially stale) Student row when no active term exists.
            // Section always comes from Student because StudentTermEnrollment has no
            // Section column — it is a class-level, not a term-level, attribute.
            // -----------------------------------------------------------------
            string? gradeLevel;
            string? schoolYear;
            int? term;

            if (currentTerm is not null)
            {
                gradeLevel = GradeLevelHelper.ToDisplay(currentTerm.GradeLevel);
                schoolYear = currentTerm.SchoolYear;
                term = currentTerm.Term;
            }
            else
            {
                gradeLevel = student.GradeLevel;
                schoolYear = student.SchoolYear;
                term = null;
            }

            return new StudentProfileDto(
                student.StudentId,
                student.StudentNumber,
                gradeLevel,
                student.Section,
                schoolYear,
                term,
                student.Status,
                subjects);
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
