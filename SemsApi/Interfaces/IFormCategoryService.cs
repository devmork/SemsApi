using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.FormCategory;

namespace SemsApi.Interfaces
{
    public interface IFormCategoryService
    {
        Task<ActionResult<List<FormCategoryResponseDto>>> GetAllAsync(int formId, bool? isActive = null);
        Task<ActionResult> CreateAsync(int formId, FormCategoryRequestDto request, int createdBy);
        Task<ActionResult> UpdateAsync(int formId, int formCategoryId, FormCategoryRequestDto request);
        Task<ActionResult> DeleteAsync(int formId, int formCategoryId);
    }
}
