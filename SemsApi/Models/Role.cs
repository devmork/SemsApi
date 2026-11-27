using Microsoft.AspNetCore.Identity;

namespace SemsApi.Models
{
    public class Role : IdentityRole<int>
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int SortOrder { get; set; }
    }
}