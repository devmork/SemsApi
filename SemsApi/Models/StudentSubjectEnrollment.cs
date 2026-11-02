namespace SemsApi.Models
{
    public class StudentSubjectEnrollment
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int SubjectOfferingId { get; set; }
        public SubjectOffering SubjectOffering { get; set; } = null!;

        /// <summary>"Enrolled" | "Dropped" | "Completed"</summary>
        public string Status { get; set; } = "Enrolled";

        public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DroppedAtUtc { get; set; }
    }
}
