using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationType;

namespace SemsApi.Interfaces
{
    public interface IEvaluationTypeService
    {
        Task<ActionResult<List<EvaluationTypeResponseDto>>> GetAllAsync(string? search = null);
        Task<ActionResult<EvaluationTypeResponseDto>> GetByIdAsync(int id);
        Task<ActionResult<EvaluationTypeResponseDto>> CreateAsync(EvaluationTypeRequestDto request);
        Task<ActionResult<EvaluationTypeResponseDto>> UpdateAsync(int id, EvaluationTypeRequestDto request);
        Task<ActionResult> DeleteAsync(int id);
    }
}