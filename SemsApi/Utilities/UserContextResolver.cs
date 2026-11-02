using Microsoft.EntityFrameworkCore;
using SemsApi.Data;

namespace SemsApi.Utilities
{
    public static class UserContextResolver
    {
        public static int? GetUserId(HttpContext http)
        {
            // --- DEVELOPMENT BYPASS ---
            var env = http.RequestServices.GetService<IWebHostEnvironment>();
            if (env != null && env.IsDevelopment())
            {
                var debugUserId = http.Request.Query["debug_userId"].FirstOrDefault();
                if (int.TryParse(debugUserId, out var parsedId))
                {
                    return parsedId;
                }
            }
            // --------------------------
            var raw = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                   ?? http.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }

        public static async Task<int?> GetStudentIdAsync(
            ApplicationDbContext ctx, HttpContext http, CancellationToken ct = default)
        {
            var userId = GetUserId(http);
            if (userId is null) return null;
            var studentId = await ctx.Students.AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync(ct);
            return studentId == 0 ? null : studentId;
        }

        public static async Task<int?> GetTeacherIdAsync(
            ApplicationDbContext ctx, HttpContext http, CancellationToken ct = default)
        {
            var userId = GetUserId(http);
            if (userId is null) return null;
            var teacherId = await ctx.Teachers.AsNoTracking()
                .Where(t => t.UserId == userId)
                .Select(t => t.TeacherId)
                .FirstOrDefaultAsync(ct);
            return teacherId == 0 ? null : teacherId;
        }
    }
}