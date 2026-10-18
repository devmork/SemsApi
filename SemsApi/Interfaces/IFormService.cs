using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using SemsApi.DTO.Form;

namespace SemsApi.Interfaces
{
    public interface IFormService
    {
        /// <param name="studentView">When true, only active forms are returned.</param>
        Task<ActionResult<List<FormDto>>> GetAllAsync(
            int? programId = null, int? evaluationTypeId = null, int? isActive = null, bool studentView = false);

        /// <param name="studentView">When true, an inactive form is treated as not found.</param>
        Task<ActionResult<FormDetailDto>> GetByIdAsync(int formId, bool studentView = false);

        /// <param name="createdBy">The id of the signed-in user creating the form.</param>
        Task<ActionResult> CreateAsync(FormRequestDto request, int createdBy);

        Task<ActionResult> UpdateAsync(int formId, FormRequestDto request);
        Task<ActionResult> DeleteAsync(int formId);
    }
}