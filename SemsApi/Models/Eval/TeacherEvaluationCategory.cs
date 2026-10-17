namespace SemsApi.Models.Eval
{
    public class TeacherEvaluationCategory
    {
        public int Recno { get; set; }

        //New foreign key to EvaluationForm
        public int FormId { get; set; }
        public EvaluationForm Form { get; set; } = null!;

        // NEW — proper FK to EvaluationType
        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public int? CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}
