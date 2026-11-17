using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Evaluation;

namespace SemsApi.Interfaces
{
    public interface IQuestionService
    {
        Task<ActionResult<List<EvaluationQuestionDto>>> GetAllAsync(int categoryId);
        Task<ActionResult<EvaluationQuestionDto>> GetByNoAsync(int categoryId, int questionId);
        Task<ActionResult> CreateAsync(int categoryId, EvaluationQuestionDto request);
        Task<ActionResult> UpdateAsync(int categoryId, int questionId, EvaluationQuestionDto request);
        Task<ActionResult> DeleteAsync(int categoryId, int questionId);
    }
}
