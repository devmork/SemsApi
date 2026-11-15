using SemsApi.DTO.Form;

namespace SemsApi.DTO.EvaluationSessions
{
    public class EvaluationSessionDetailDto : EvaluationSessionDto
    {
        /// <summary>The underlying EvaluationForm with categories and questions.</summary>
        public FormDetailDto? EvaluationForm { get; set; }
    }
}
