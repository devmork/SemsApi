namespace SemsApi.DTO.Authentication;

public sealed record TokenResponse(string AccessToken, DateTime AccessExpiresAtUtc, string RefreshToken, DateTime RefreshExpiresAtUtc);
