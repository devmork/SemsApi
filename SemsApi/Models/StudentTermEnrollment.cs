namespace SemsApi.Models
{
    public class StudentTermEnrollment
    {
        public int StudentTermEnrollmentId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public int GradeLevel { get; set; }
        public string Status { get; set; } = "Enrolled";
        public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
    }
}
