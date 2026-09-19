namespace SemsApi.DTO
{
    public class SubmitTeacherStrengthRequest //Purpose: Payload when submitting teacher comments
    {
        public string EvalType { get; set; } = "Teacher Evaluation";
        public string TeacherId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Sy { get; set; } = "2026-2027";
        public int Term { get; set; } = 1;
        public string Comment { get; set; } = string.Empty;
    }
}
