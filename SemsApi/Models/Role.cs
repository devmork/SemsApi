using Microsoft.AspNetCore.Identity;

namespace SemsApi.Models
{
    public class Role : IdentityRole<int>
    {
        /// <summary>
        /// Friendly description of the role (e.g. "System Administrator", "Classroom Teacher")
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether this role is currently active and can be assigned
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the role was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional display order for UI sorting
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}