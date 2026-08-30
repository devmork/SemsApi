namespace SemsApi.Models
{
    public class AuthorizedEmailDomain
    {
        public int DomainId { get; set; }
        public string Domain { get; set; } = string.Empty; // "dmc.edu.ph" (no @)
        public string InstitutionName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
