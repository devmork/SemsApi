namespace SemsApi.DTO.Teacher
{
    public class TeacherStrengthDto //Purpose: Returning saved teacher comments
    {
        public int Recno { get; set; }
        public int EvaluationTypeId { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public string? CommentName { get; set; }
    }
}