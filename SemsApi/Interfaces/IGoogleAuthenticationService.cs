using Google.Apis.Auth;

namespace SemsApi.Interfaces
{
    public interface IGoogleAuthenticationService
    {
        Task<GoogleJsonWebSignature.Payload?> ValidateAsync(string idToken);
    }
}
