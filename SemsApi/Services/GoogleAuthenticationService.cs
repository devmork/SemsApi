using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SemsApi.Configuration;
using SemsApi.DTO;
using SemsApi.Interfaces;
using System.Text.Json;

namespace SemsApi.Services;

public class GoogleAuthenticationService : IGoogleAuthenticationService
{
    private readonly GoogleOptions _options;

    public GoogleAuthenticationService(IOptions<GoogleOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code)
    {
        var tokenEndpoint = "https://oauth2.googleapis.com/token";
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("code", code),
            new("client_id", _options.ClientId),
            new("client_secret", _options.ClientSecret),
            new("redirect_uri", _options.RedirectUri),
            new("grant_type", "authorization_code")
        };

        using var client = new HttpClient();
        var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return tokenResponse;
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