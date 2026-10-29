namespace SemsApi.DTO.Auth
{
    public class ContinueWithGoogleRequest
    {
        public string Code { get; set; } = string.Empty;
        public string? CodeVerifier { get; set; }
    }
}