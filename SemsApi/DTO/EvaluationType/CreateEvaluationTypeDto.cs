using System.ComponentModel.DataAnnotations;

namespace SemsApi.DTO.EvaluationType
{
    public class CreateEvaluationTypeDto
    {
        public int EvaluationTypeId { get; set; }
        public string? EvaluationTypeName { get; set; }
    }
}