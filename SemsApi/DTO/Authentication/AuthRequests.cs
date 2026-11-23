namespace SemsApi.DTO.Authentication;

/// <summary>Body of POST /api/auth/refresh. Tokens travel in the body, never in a cookie.</summary>
public sealed record RefreshRequest(string? RefreshToken);

/// <summary>Body of POST /api/auth/logout.</summary>
public sealed record LogoutRequest(string? RefreshToken);

/// <summary>
/// Body of POST /api/auth/exchange. Holds the single-use, short-lived code that
/// the Google callback put on the frontend redirect URL.
/// </summary>
public sealed record ExchangeRequest(string? Code);
