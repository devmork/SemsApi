using Google.Apis.Auth.OAuth2.Responses;

namespace SemsApi.Utilities
{
    public static class CookiesUtility
    {
        public const string AccessToken = "ACCESS_TOKEN";
        public const string RefreshToken = "REFRESH_TOKEN";
        private const string RefreshPath = "/api/auth";

        public static void Append(HttpResponse res, TokenResponse t)
        {
            res.Cookies.Append(AccessToken, t.AccessToken, Options(t.AccessExpiresAtUtc, "/"));
            res.Cookies.Append(RefreshToken, t.RefreshToken, Options(t.RefreshExpiresAtUtc, RefreshPath));
        }

        public static void Clear(HttpResponse res)
        {
            res.Cookies.Delete(AccessToken, Options(null, "/"));
            res.Cookies.Delete(RefreshToken, Options(null, RefreshPath));
        }

        private static CookieOptions Options(DateTime? expiresUtc, string path) => new()
        {
            HttpOnly = true,                 // JS can never read it
            Secure = true,                   // HTTPS only
            SameSite = SameSiteMode.Lax,     // blocks cross-site POSTs, allows the Google redirect back
            Path = path,
            IsEssential = true,
            Expires = expiresUtc is null ? null : new DateTimeOffset(expiresUtc.Value)
        };
    }
}
