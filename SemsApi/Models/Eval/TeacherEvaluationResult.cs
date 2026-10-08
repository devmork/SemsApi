namespace SemsApi.Models.Eval
{
    public class TeacherEvaluationResult
    {
        public int Recno { get; set; }

        //Mew foreign Key
        public int FormId { get; set; }
        public EvaluationForm Form { get; set; } = null!;

        public string? EvalType { get; set; } //Keep it for now for backward compatibility, but we will use FormId to determine the form type in the future
        public int? CatNo { get; set; }
        public int? QnNo { get; set; }
        public string? QnName { get; set; }
    }
}
