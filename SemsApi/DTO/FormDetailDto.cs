using Microsoft.SqlServer.Server;
using SemsApi.DTO.Evaluation;

namespace SemsApi.DTO
{
    /// <summary>
    /// A form together with its categories and the questions inside each category.
    /// Returned by GET api/forms/{formId}.
    /// </summary>
    public class FormDetailDto : FormDto
    {
        public List<EvaluationCategoryDto> Categories { get; set; } = new();
    }
}