namespace SemsApi.DTO
{
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }
}
