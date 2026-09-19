using Microsoft.AspNetCore.Identity;

namespace SemsApi.Models
{
    public class Role : IdentityRole<int> //Inherits from IdentityRole with int as the primary key type
    {
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}