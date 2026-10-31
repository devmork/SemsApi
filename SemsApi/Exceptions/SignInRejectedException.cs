namespace SemsApi.Exceptions
{
    public static class SignInErrorCodes
    {
        public const string GoogleAuthFailed = "google_auth_failed";
        public const string EmailNotVerified = "email_not_verified";
        public const string DomainNotAllowed = "domain_not_allowed";
        public const string AccountConflict = "account_conflict";
        public const string AccountInactive = "account_inactive";
    }

    public class SignInRejectedException(string code) : Exception($"Sign-in rejected: {code}")
    {
        public string Code { get; } = code;
    }
}
