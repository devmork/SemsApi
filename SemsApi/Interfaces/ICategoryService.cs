using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;

namespace SemsApi.Interfaces
{
    public interface ICategoryService
    {
        Task<ActionResult<List<CategoryResponseDto>>> GetAllAsync(int? evaluationTypeId = null);
        Task<ActionResult<CategoryResponseDto>> GetByIdAsync(int categoryId);
        Task<ActionResult> CreateAsync(CategoryRequestDto request);
        Task<ActionResult> UpdateAsync(int categoryId, CategoryRequestDto request);
        Task<ActionResult> DeleteAsync(int categoryId);
    }
}