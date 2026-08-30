namespace SemsApi.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty; //"Admin" , "Teacher" , "Student"
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
