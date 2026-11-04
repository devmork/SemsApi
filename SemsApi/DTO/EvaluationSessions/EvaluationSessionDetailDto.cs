using SemsApi.DTO.Form;

namespace SemsApi.DTO.EvaluationSessions
{
    public class EvaluationSessionDetailDto : EvaluationSessionDto
    {
        /// <summary>The underlying Form with categories and questions.</summary>
        public FormDetailDto? Form { get; set; }
    }
}
