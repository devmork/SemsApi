using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Evaluation;

namespace SemsApi.Interfaces
{
    public interface IQuestionService
    {
        Task<ActionResult<List<EvaluationQuestionDto>>> GetAllAsync(int catNo);
        Task<ActionResult<EvaluationQuestionDto>> GetByNoAsync(int catNo, int qnNo);
        Task<ActionResult> CreateAsync(int catNo, EvaluationQuestionDto request);
        Task<ActionResult> UpdateAsync(int catNo, int qnNo, EvaluationQuestionDto request);
        Task<ActionResult> DeleteAsync(int catNo, int qnNo);
    }
}
