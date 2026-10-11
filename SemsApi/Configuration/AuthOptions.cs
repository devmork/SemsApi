namespace SemsApi.Configuration
{
    public class AuthOptions
    {
        public string[] AuthorizedDomains { get; set; } = Array.Empty<string>();
        public string InstitutionName { get; set; } = string.Empty;
    }
}
