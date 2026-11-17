using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Users;
using SemsApi.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAll(
            [FromQuery] string? search, [FromQuery] string? role, CancellationToken ct)
            => await userService.GetAllAsync(search, role, ct);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
            => await userService.GetByIdAsync(id, ct);

        /// <summary>
        /// Replaces the user's roles (promote/demote). Roles must exist (Admin, QA, Student);
        /// admins cannot remove their own Admin role.
        /// </summary>
        [HttpPut("{id:int}/roles")]
        public async Task<ActionResult<UserDto>> UpdateRoles(int id, UpdateUserRolesRequest request, CancellationToken ct)
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(idValue, out var actingUserId))
                return Unauthorized();

            return await userService.UpdateRolesAsync(id, request, actingUserId, ct);
        }
    }
}
