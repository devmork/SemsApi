using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Auth;
using SemsApi.DTO.User;

namespace SemsApi.Interfaces
{
    public interface IAuthService
    {

        Task<ActionResult<LoginResponseDto>> ContinueWithGoogleAsync(ContinueWithGoogleRequest request);
        Task<ActionResult<UserProfileDto>> GetUserProfileAsync();
        Task LogoutAsync();
        Task SetRefreshTokenCookieAsync(string refreshToken);
        Task<ActionResult<LoginResponseDto>> RefreshAsync(string refreshToken);
        Task<GoogleJsonWebSignature.Payload?> ValidateGoogleIdTokenAsync(string idToken);
        Task<GoogleTokenResponse?> ExchangeGoogleAuthCodeForTokensAsync(string code, string codeVerifier);
    }
}
