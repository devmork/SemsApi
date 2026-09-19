using Microsoft.AspNetCore.Identity;

namespace SemsApi.Models
{
    public class User : IdentityUser<int> //Inherits from IdentityUser with int as the primary key type
    {
        public string GoogleSubjectId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
    }
}