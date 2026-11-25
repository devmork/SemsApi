using System.ComponentModel.DataAnnotations;

namespace SemsApi.DTO.EvaluationType
{
    public class CreateEvaluationTypeDto
    {
        [Required(AllowEmptyStrings = false)]
        public string? EvaluationTypeName { get; set; }
    }
}