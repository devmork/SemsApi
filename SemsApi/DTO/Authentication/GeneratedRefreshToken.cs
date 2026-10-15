namespace SemsApi.DTO.Authentication;

public sealed record GeneratedRefreshToken(string Token, string Hash, DateTime ExpiresAtUtc);