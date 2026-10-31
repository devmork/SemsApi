using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Form;

namespace SemsApi.Interfaces
{
    public interface IFormService
    {
        Task<ActionResult<List<FormDto>>> GetAllAsync(
            int? programId = null, int? evaluationTypeId = null, int? isActive = null, bool studentView = false);

        Task<ActionResult<FormDetailDto>> GetByIdAsync(int formId, bool studentView = false);

        Task<ActionResult<FormDto>> GetByCodeAsync(string code, bool studentView = false);

        Task<ActionResult> CreateAsync(FormRequestDto request, int createdBy);
        Task<ActionResult> UpdateAsync(int formId, FormRequestDto request);
        Task<ActionResult> DeleteAsync(int formId);
    }
}