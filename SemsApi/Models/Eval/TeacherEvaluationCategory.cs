namespace SemsApi.Models.Eval
{
    public class TeacherEvaluationCategory
    {
        public int Recno { get; set; }

        //New foreign key to EvaluationForm
        public int FormId { get; set; }
        public EvaluationForm Form { get; set; } = null!;

        public string? EvalType { get; set; } //Keep it for now for backward compatibility, but we will use FormId to determine the form type in the future
        public int? CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}
