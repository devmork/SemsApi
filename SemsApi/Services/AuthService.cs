using Azure;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SemsApi.Configuration;
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
        private readonly IOptions<AuthOptions> _authOptions;
        private readonly IMemoryCache _cache;

        private const string SignInCodePrefix = "signin-code:";
        private static readonly TimeSpan SignInCodeLifetime = TimeSpan.FromSeconds(60);

        public AuthService(ITokenService tokenService, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IOptions<AuthOptions> authOptions, IMemoryCache cache)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _dbContext = dbContext;
            _authOptions = authOptions;
            _cache = cache;
        }

        public Task<string> CreateSignInCodeAsync(int userId, CancellationToken ct = default)
        {
            // 256 bits of entropy, URL-safe (the code is a query-string value).
            var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            _cache.Set(SignInCodePrefix + code, userId, SignInCodeLifetime);
            return Task.FromResult(code);
        }

        public Task<int?> RedeemSignInCodeAsync(string? code, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Task.FromResult<int?>(null);

            var key = SignInCodePrefix + code;
            if (!_cache.TryGetValue<int>(key, out var userId))
                return Task.FromResult<int?>(null);

            // Single use: remove before returning so a replay can never succeed.
            _cache.Remove(key);
            return Task.FromResult<int?>(userId);
        }

        public async Task<ApplicationUser?> GetActiveUserAsync(int userId, CancellationToken ct = default)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            return user is null || user.Status != UserStatus.Active ? null : user;
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

            var authorizedDomains = _authOptions.Value.AuthorizedDomains ?? Array.Empty<string>();
            var domain = email.Split('@').LastOrDefault();
            if (string.IsNullOrEmpty(domain) || !authorizedDomains.Contains(domain, StringComparer.OrdinalIgnoreCase))
                throw new SignInRejectedException(SignInErrorCodes.DomainNotAllowed);

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

                // Bootstrap admin: if no account holds the Admin role yet, the very
                // first user to sign in becomes the Admin instead of a Student.
                var initialRole = await HasAnyAdminAsync(_dbContext)
                    ? "Student"
                    : "Admin";

                var role = await _userManager.AddToRoleAsync(user, initialRole);
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

            // Ensure a Student profile exists for Student-role accounts so they can
            // evaluate facilities even before DMC academic records are imported.
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any(r => string.Equals(r, "Student", StringComparison.OrdinalIgnoreCase)))
            {
                var existingStudent = await _dbContext.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (existingStudent is null)
                {
                    var studentNumber = await GenerateStudentNumberAsync(_dbContext);
                    _dbContext.Students.Add(new Student
                    {
                        UserId = user.Id,
                        StudentNumber = studentNumber,
                        GradeLevel = "N/A",
                        Section = "N/A",
                        SchoolYear = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}",
                        Status = "Active",
                        IsPlaceholder = true,
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }

            return new GoogleSignInResponse(user, isNew);
        }
        /// <summary>
        /// True when at least one account already holds the Admin role — i.e. the
        /// bootstrap admin has been claimed and later sign-ups stay Students.
        /// </summary>
        private static async Task<bool> HasAnyAdminAsync(ApplicationDbContext context)
        {
            var adminRoleId = await context.Roles
                .AsNoTracking()
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (adminRoleId == 0) return false;

            return await context.UserRoles.AsNoTracking()
                .AnyAsync(ur => ur.RoleId == adminRoleId);
        }

        private async Task<string> GenerateStudentNumberAsync(ApplicationDbContext context)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"{year}-";

            for (var attempt = 0; attempt < 5; attempt++)
            {
                var last = await context.Students
                    .AsNoTracking()
                    .Where(s => s.StudentNumber.StartsWith(prefix))
                    .OrderByDescending(s => s.StudentNumber)
                    .Select(s => s.StudentNumber)
                    .FirstOrDefaultAsync();

                var next = 1;
                if (last is not null && last.Length > prefix.Length &&
                    int.TryParse(last.AsSpan(prefix.Length), out var parsed))
                {
                    next = parsed + 1;
                }

                var candidate = $"{prefix}{next:D4}";

                var taken = await context.Students.AnyAsync(s => s.StudentNumber == candidate);
                if (!taken) return candidate;
            }

            throw new InvalidOperationException("Could not generate a unique student number.");
        }
    }
}
