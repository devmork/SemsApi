using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SemsApi.Exceptions;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
    {
        private readonly string _frontendUrl = configuration["Frontend:BaseUrl"]!.TrimEnd('/');

        [HttpGet("google/signin")]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl)
        {
            var props = new AuthenticationProperties { RedirectUri = "/api/auth/google/callback" };
            props.Items["returnUrl"] = RedirectUtility.SafeReturnUrl(returnUrl);
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback(CancellationToken ct)
        {
            var external = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!external.Succeeded)
                return Redirect(RedirectUtility.LoginErrorUrl(_frontendUrl, SignInErrorCodes.GoogleAuthFailed));

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            try
            {
                var result = await authService.SignInWithGoogleAsync(external.Principal);
                var tokens = await authService.IssueTokensAsync(result.applicationUser, ct);
                CookiesUtility.Append(Response, tokens);

                external.Properties!.Items.TryGetValue("returnUrl", out var returnUrl);
                return Redirect($"{_frontendUrl}{RedirectUtility.SafeReturnUrl(returnUrl)}");
            }
            catch (SignInRejectedException ex)
            {
                return Redirect(RedirectUtility.LoginErrorUrl(_frontendUrl, ex.Code));
            }
            catch (RegistrationFailedException)
            {
                return Redirect(RedirectUtility.LoginErrorUrl(_frontendUrl, SignInErrorCodes.GoogleAuthFailed));
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(idValue, out var userId))
                return Unauthorized();

            var me = await authService.GetCurrentUserAsync(userId, ct);
            return me is null ? Unauthorized() : Ok(me);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            try
            {
                var tokens = await authService.RefreshTokenAsync(Request.Cookies[CookiesUtility.RefreshToken], ct);
                CookiesUtility.Append(Response, tokens);
                return Ok(new { accessTokenExpiresAtUtc = tokens.AccessExpiresAtUtc });
            }
            catch (RefreshTokenException)
            {
                CookiesUtility.Clear(Response);
                throw;   // GlobalExceptionHandler -> 401
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            await authService.LogoutAsync(Request.Cookies[CookiesUtility.RefreshToken], ct);
            CookiesUtility.Clear(Response);
            return NoContent();
        }
    }
}