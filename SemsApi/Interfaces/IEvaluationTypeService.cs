using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationType;

namespace SemsApi.Interfaces
{
    public interface IEvaluationTypeService
    {
        Task<ActionResult<List<EvaluationTypeDto>>> GetAllAsync(string? search = null);
        Task<ActionResult<EvaluationTypeDto>> GetByIdAsync(int id);
        Task<ActionResult<CreateEvaluationTypeDto>> CreateAsync(CreateEvaluationTypeDto dto);
        Task<ActionResult<UpdateEvaluationTypeDto>> UpdateAsync(int id, UpdateEvaluationTypeDto dto);
        Task<ActionResult> DeleteAsync(int id);
    }
}