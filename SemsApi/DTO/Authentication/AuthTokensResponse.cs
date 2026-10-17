namespace SemsApi.DTO.Authentication;

public sealed record AuthTokenResponse
    (string AccessToken, DateTime AccessExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshExpiresAtUtc);