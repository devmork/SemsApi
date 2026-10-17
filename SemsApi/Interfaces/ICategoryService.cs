using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;

namespace SemsApi.Interfaces
{
    public interface ICategoryService
    {
        Task<ActionResult<List<CategoryResponseDto>>> GetAllAsync(string? evalType = null, int? formId = null);
        Task<ActionResult<CategoryResponseDto>> GetByIdAsync(int recno);
        Task<ActionResult> CreateAsync(CategoryRequestDto request);
        Task<ActionResult> UpdateAsync(int recno, CategoryRequestDto request);
        Task<ActionResult> DeleteAsync(int recno);
    }
}