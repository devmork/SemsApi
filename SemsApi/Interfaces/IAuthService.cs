using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;

namespace SemsApi.Interfaces
{
    public interface IAuthService
    {

        Task<ActionResult<LoginResponseDto>> GoogleCallbackAsync(GoogleCallbackRequest request);
        Task<ActionResult<LoginResponseDto>> GoogleLoginAsync(string idToken);
        Task<ActionResult<UserProfileDto>> GetUserProfileAsync();
        Task LogoutAsync();
        Task SetRefreshTokenCookieAsync(string refreshToken);
        Task<ActionResult<LoginResponseDto>> RefreshAsync(string refreshToken);
        Task<GoogleJsonWebSignature.Payload?> ValidateGoogleIdTokenAsync(string idToken);
        Task<GoogleTokenResponse?> ExchangeGoogleAuthCodeForTokensAsync(string code);
    }
}
