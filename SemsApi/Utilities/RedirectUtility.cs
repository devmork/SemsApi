namespace SemsApi.Utilities
{
    public static class RedirectUtility
    {
        /// <summary>
        /// Only same-site relative paths are allowed.
        /// Blocks "@evil.com", "//evil.com" and "/\evil.com".
        /// </summary>
        public static string SafeReturnUrl(string? url) =>
            !string.IsNullOrWhiteSpace(url) && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\")
                ? url
                : "/";

        /// <summary>Builds the frontend login URL carrying the error code, e.g. /login?error=domain_not_allowed</summary>
        public static string LoginErrorUrl(string frontendUrl, string code) =>
            $"{frontendUrl}/login?error={Uri.EscapeDataString(code)}";
    }
}