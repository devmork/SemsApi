using System.ComponentModel.DataAnnotations;

namespace SemsApi.DTO.EvaluationType
{
    public class UpdateEvaluationTypeDto
    {
        [Required(AllowEmptyStrings = false)]
        public string? EvaluationTypeName { get; set; }

    }
}
