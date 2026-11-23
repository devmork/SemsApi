using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Users;
using SemsApi.Interfaces;
using SemsApi.Models;

namespace SemsApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ActionResult<List<UserDto>>> GetAllAsync(
            string? search = null, string? role = null, CancellationToken ct = default)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.Email!.Contains(term) ||
                    u.FirstName.Contains(term) ||
                    (u.MiddleName != null && u.MiddleName.Contains(term)) ||
                    u.LastName.Contains(term));
            }

            var users = await query
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .ToListAsync(ct);

            var data = new List<UserDto>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!string.IsNullOrWhiteSpace(role) &&
                    !roles.Contains(role.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }
                data.Add(MapToDto(user, roles));
            }

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return new NotFoundObjectResult(new { message = "User not found." });

            var roles = await _userManager.GetRolesAsync(user);
            return new OkObjectResult(MapToDto(user, roles));
        }

        public async Task<ActionResult<UserDto>> UpdateRolesAsync(
            int id, UpdateUserRolesRequest request, int actingUserId, CancellationToken ct = default)
        {
            if (request.Roles is null || request.Roles.Count == 0)
                return new BadRequestObjectResult(new { message = "At least one role is required." });

            // Roles are case-insensitive; duplicates removed.
            var requestedRoles = request.Roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var validRoleNames = (await _context.Roles.ToListAsync(ct))
                .Select(r => r.Name!).ToList();

            var unknown = requestedRoles.Except(validRoleNames, StringComparer.OrdinalIgnoreCase).ToList();
            if (unknown.Count > 0)
                return new BadRequestObjectResult(new { message = $"Unknown role(s): {string.Join(", ", unknown)}." });

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return new NotFoundObjectResult(new { message = "User not found." });

            // Guard: an admin cannot remove their own Admin role (avoid locking out the last admin).
            if (user.Id == actingUserId)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains("Admin") && !requestedRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                    return new BadRequestObjectResult(new { message = "You cannot remove your own Admin role." });
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            var targetRoles = requestedRoles
                .Select(r => validRoleNames.First(v => string.Equals(v, r, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var toAdd = targetRoles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var toRemove = existingRoles.Except(targetRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (toRemove.Count > 0)
            {
                var result = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!result.Succeeded)
                    return new ObjectResult(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
            }

            if (toAdd.Count > 0)
            {
                var result = await _userManager.AddToRolesAsync(user, toAdd);
                if (!result.Succeeded)
                    return new ObjectResult(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
            }

            var updatedRoles = await _userManager.GetRolesAsync(user);
            return new OkObjectResult(MapToDto(user, updatedRoles));
        }

        private static UserDto MapToDto(ApplicationUser user, IList<string> roles) => new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            PictureUrl = user.PictureUrl,
            Status = user.Status.ToString(),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            Roles = [.. roles]
        };
    }
}
