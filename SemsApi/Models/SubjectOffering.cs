namespace SemsApi.Models
{
    public class SubjectOffering
    {
        public int SubjectOfferingId { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public string ProgramCode { get; set; } = string.Empty;
        public string EdpCode { get; set; } = string.Empty;

        /// <summary>0=Nursery, 1=Kinder1, 2=Kinder2, 3=G1, ..., 14=G12.</summary>
        public int GradeLevel { get; set; }
        public string Section { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public int Term { get; set; }

        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public int? AdviserId { get; set; }
        public Teacher? Adviser { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<StudentSubjectEnrollment> Enrollments { get; set; } = new List<StudentSubjectEnrollment>();
    }
}
