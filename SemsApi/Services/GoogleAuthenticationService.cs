using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SemsApi.Configuration;
using SemsApi.Interfaces;

namespace SemsApi.Services;

public class GoogleAuthenticationService : IGoogleAuthenticationService
{
    private readonly GoogleOptions _options;

    public GoogleAuthenticationService(IOptions<GoogleOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleJsonWebSignature.Payload?> ValidateAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            };
            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException)
        {
            return null; // invalid signature, expired, wrong audience, etc.
        }
    }
}