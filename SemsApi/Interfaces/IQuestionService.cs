using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.FormQuestion;

namespace SemsApi.Interfaces
{
    public interface IQuestionService
    {
        Task<ActionResult<List<FormQuestionResponseDto>>> GetAllAsync(int formId, int categoryId);
        Task<ActionResult<FormQuestionResponseDto>> GetByNoAsync(int formId, int categoryId, int questionId);
        Task<ActionResult> CreateAsync(int formId, int categoryId, FormQuestionRequestDto request, int createdBy);
        Task<ActionResult> UpdateAsync(int formId, int categoryId, int questionId, FormQuestionRequestDto request);
        Task<ActionResult> DeleteAsync(int formId, int categoryId, int questionId);
    }
}
