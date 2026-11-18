using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.FormCategory;
using SemsApi.Interfaces;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    /// <summary>
    /// Manages which shared evaluation categories belong to a form.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "QA,Admin")]
    [Route("api/evaluation/forms/{formId:int}/categories")]
    public class FormCategoryController : ControllerBase
    {
        private readonly IFormCategoryService _service;

        public FormCategoryController(IFormCategoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// List the categories selected for a form. Optionally filter by isActive (true/false).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FormCategoryResponseDto>>> GetAll(int formId, [FromQuery] bool? isActive = null)
        {
            return await _service.GetAllAsync(formId, isActive);
        }

        /// <summary>
        /// Add a shared category to a form. DisplayOrder is auto-assigned when omitted.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create(int formId, [FromBody] FormCategoryRequestDto request)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null)
                return Unauthorized(new { message = "Sign in to add a category." });

            return await _service.CreateAsync(formId, request, userId.Value);
        }

        /// <summary>
        /// Update a form category's active state or display order.
        /// </summary>
        [HttpPut("{formCategoryId:int}")]
        public async Task<ActionResult> Update(int formId, int formCategoryId, [FromBody] FormCategoryRequestDto request)
        {
            return await _service.UpdateAsync(formId, formCategoryId, request);
        }

        /// <summary>
        /// Remove a category from a form. Fails while the category still has questions.
        /// </summary>
        [HttpDelete("{formCategoryId:int}")]
        public async Task<ActionResult> Delete(int formId, int formCategoryId)
        {
            return await _service.DeleteAsync(formId, formCategoryId);
        }
    }
}
