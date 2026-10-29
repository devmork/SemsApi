using Microsoft.AspNetCore.Identity;

namespace SemsApi.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string GoogleSubjectId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
    }
}