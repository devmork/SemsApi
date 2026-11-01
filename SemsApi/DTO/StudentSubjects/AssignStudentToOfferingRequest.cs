namespace SemsApi.DTO.StudentSubjects
{
    public class AssignStudentToOfferingRequest
    {
        public int StudentId { get; set; }
        public int SubjectOfferingId { get; set; }
    }
}