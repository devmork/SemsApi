using Google.Apis.Auth;
using SemsApi.DTO;

namespace SemsApi.Interfaces
{
    public interface IGoogleAuthenticationService
    {
        Task<GoogleJsonWebSignature.Payload?> ValidateAsync(string idToken);
        Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code);
    }
}
