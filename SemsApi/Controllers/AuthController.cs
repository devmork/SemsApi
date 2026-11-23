using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using SemsApi.DTO.Authentication;
using SemsApi.Exceptions;
using SemsApi.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
    {
        private readonly string _frontendUrl = configuration["Frontend:BaseUrl"]!.TrimEnd('/');

        [HttpGet("google/signin")]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl)
        {
            var props = new AuthenticationProperties { RedirectUri = "/api/auth/google/callback" };
            props.Items["returnUrl"] = Utilities.RedirectUtility.SafeReturnUrl(returnUrl);
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback(CancellationToken ct)
        {
            var external = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!external.Succeeded)
                return Redirect(Utilities.RedirectUtility.LoginErrorUrl(_frontendUrl, SignInErrorCodes.GoogleAuthFailed));

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            try
            {
                var result = await authService.SignInWithGoogleAsync(external.Principal);

                // Tokens never travel in a URL or a cookie: the browser only carries a
                // single-use code for 60s and exchanges it for tokens over the API.
                var code = await authService.CreateSignInCodeAsync(result.applicationUser.Id, ct);

                external.Properties!.Items.TryGetValue("returnUrl", out var returnUrl);
                var target = QueryHelpers.AddQueryString(SafeFrontendPath(returnUrl), "code", code);
                return Redirect(target);
            }
            catch (SignInRejectedException ex)
            {
                return Redirect(Utilities.RedirectUtility.LoginErrorUrl(_frontendUrl, ex.Code));
            }
            catch (RegistrationFailedException)
            {
                return Redirect(Utilities.RedirectUtility.LoginErrorUrl(_frontendUrl, SignInErrorCodes.GoogleAuthFailed));
            }
        }

        /// <summary>Redeems the one-time code from the callback and returns a fresh token pair.</summary>
        [HttpPost("exchange")]
        public async Task<IActionResult> Exchange([FromBody] ExchangeRequest request, CancellationToken ct)
        {
            var userId = await authService.RedeemSignInCodeAsync(request.Code, ct);
            if (userId is null)
                return UnauthorizedProblem("Invalid or expired sign-in code.");

            var user = await authService.GetActiveUserAsync(userId.Value, ct);
            if (user is null)
                return UnauthorizedProblem("Account is not active.");

            var tokens = await authService.IssueTokensAsync(user, ct);
            return Ok(TokenPayload(tokens));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(idValue, out var userId))
                return UnauthorizedProblem("Invalid access token.");

            var me = await authService.GetCurrentUserAsync(userId, ct);
            return me is null ? UnauthorizedProblem("Account is not active.") : Ok(me);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
        {
            try
            {
                var tokens = await authService.RefreshTokenAsync(request.RefreshToken, ct);
                return Ok(TokenPayload(tokens));
            }
            catch (RefreshTokenException)
            {
                // RFC 9110: a 401 must carry a challenge.
                return UnauthorizedProblem("Invalid or expired refresh token.");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
        {
            await authService.LogoutAsync(request.RefreshToken, ct);
            return NoContent();
        }

        private IActionResult UnauthorizedProblem(string title)
        {
            Response.Headers.WWWAuthenticate = "Bearer";
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = title,
            });
        }

        private static object TokenPayload(AuthTokenResponse tokens) => new
        {
            accessToken = tokens.AccessToken,
            refreshToken = tokens.RefreshToken,
            accessExpiresAtUtc = tokens.AccessExpiresAtUtc,
            refreshExpiresAtUtc = tokens.RefreshExpiresAtUtc,
        };

        /// <summary>
        /// Turns a validated relative return path into an absolute frontend URL.
        /// Uses the framework's open-redirect check and drops any fragment so the
        /// code query string can never be appended after a '#'.
        /// </summary>
        private string SafeFrontendPath(string? returnUrl)
        {
            var path = !string.IsNullOrWhiteSpace(returnUrl) && RedirectHttpResult.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/";

            var hash = path.IndexOf('#');
            if (hash >= 0)
                path = path[..hash];

            return $"{_frontendUrl}{path}";
        }
    }
}
