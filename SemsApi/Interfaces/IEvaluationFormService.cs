using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Evaluation;

namespace SemsApi.Interfaces
{
    public interface IEvaluationFormService
    {
        Task<ActionResult<List<EvaluationFormDto>>> GetAllAsync(bool onlyActive = false);
        Task<ActionResult<EvaluationFormDto>> GetByIdAsync(int id);
        Task<ActionResult<EvaluationFormDto>> GetByCodeAsync(string code);
        Task<ActionResult> CreateAsync(EvaluationFormRequestDto request);
        Task<ActionResult> UpdateAsync(int id, EvaluationFormRequestDto request);
        Task<ActionResult> SoftDeleteAsync(int id);
        Task<ActionResult> HardDeleteAsync(int id);
    }
}