namespace SemsApi.DTO
{
    public class TeacherStrengthDto //Purpose: Returning saved teacher comments
    {
        public int Recno { get; set; }
        public string? EvalType { get; set; }
        public string? TeacherId { get; set; }
        public string? StudentId { get; set; }
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public string? CommentName { get; set; }
    }
}
