using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;
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

        /// <summary>
        /// Google OAuth Callback (Authorization Code flow)
        /// </summary>
        [HttpPost("google-callback")]
        public async Task<ActionResult<LoginResponseDto>> GoogleCallback([FromBody] GoogleCallbackRequest request)
        {
            return await _authService.GoogleCallbackAsync(request);
        }

        /// <summary>
        /// Google Login using ID Token (common for mobile / SPA)
        /// </summary>
        [HttpPost("google")]
        public async Task<ActionResult<LoginResponseDto>> Google([FromBody] GoogleLoginRequest request)
        {
            return await _authService.GoogleLoginAsync(request.IdToken);
        }

        /// <summary>
        /// Get current logged-in user profile
        /// </summary>
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