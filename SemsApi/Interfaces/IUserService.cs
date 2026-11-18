using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Users;

namespace SemsApi.Interfaces
{
    public interface IUserService
    {
        Task<ActionResult<List<UserDto>>> GetAllAsync(
            string? search = null, string? role = null, CancellationToken ct = default);
        Task<ActionResult<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ActionResult<UserDto>> UpdateRolesAsync(
            int id, UpdateUserRolesRequest request, int actingUserId, CancellationToken ct = default);
    }
}
