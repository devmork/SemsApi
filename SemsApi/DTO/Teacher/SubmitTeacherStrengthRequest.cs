namespace SemsApi.DTO.Teacher
{
    public class SubmitTeacherStrengthRequest //Purpose: Payload when submitting teacher comments
    {
        public int EvaluationTypeId { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public string Sy { get; set; } = "2026-2027";
        public int Term { get; set; } = 1;
        public string Comment { get; set; } = string.Empty;
    }
}