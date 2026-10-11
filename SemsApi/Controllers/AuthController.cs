using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Auth;
using SemsApi.DTO.User;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("google")]
        public async Task<ActionResult<LoginResponseDto>> ContinueWithGoogle([FromBody] ContinueWithGoogleRequest request)
        {
            return await _authService.ContinueWithGoogleAsync(request);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> Me()
        {
            return await _authService.GetUserProfileAsync();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponseDto>> Refresh()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No refresh token." });

            return await _authService.RefreshAsync(refreshToken);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok();
        }
    }
}