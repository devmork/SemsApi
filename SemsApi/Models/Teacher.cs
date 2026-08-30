namespace SemsApi.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string EmployeeNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }
}
