using Microsoft.AspNetCore.Identity;

namespace SemsApi.Models
{
    public class Role : IdentityRole<int> //Inherits from IdentityRole with int as the primary key type
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty; //"Admin" , "Teacher" , "Student"
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
