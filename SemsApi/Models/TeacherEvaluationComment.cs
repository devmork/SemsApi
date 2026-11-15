namespace SemsApi.Models
{
    public class TeacherEvaluationComment
    {
        public int Recno { get; set; }

        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public string? Sy { get; set; }
        public int? Term { get; set; }
        public string? CommentName { get; set; }
    }
}