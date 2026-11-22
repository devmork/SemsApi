using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.FormQuestion;
using SemsApi.Interfaces;
using SemsApi.Utilities;

namespace SemsApi.Controllers
{
    /// <summary>
    /// CRUD for form-scoped questions. Questions belong to an EvaluationFormCategory,
    /// identified here by the form and the shared category it selects.
    /// All logic lives in IQuestionService; this controller only handles HTTP concerns.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/evaluation/forms/{formId:int}/categories/{categoryId:int}/questions")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;

        public QuestionController(IQuestionService service)
        {
            _service = service;
        }

        /// <summary>
        /// List the questions of a form category, ordered by DisplayOrder.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FormQuestionResponseDto>>> GetAll(int formId, int categoryId)
        {
            return await _service.GetAllAsync(formId, categoryId);
        }

        /// <summary>
        /// Get a single question of a form category by FormQuestionId.
        /// </summary>
        [HttpGet("{questionId:int}")]
        public async Task<ActionResult<FormQuestionResponseDto>> GetByNo(int formId, int categoryId, int questionId)
        {
            return await _service.GetByNoAsync(formId, categoryId, questionId);
        }

        /// <summary>
        /// Add a question to a form category. FormQuestionId is assigned by the database.
        /// DisplayOrder is auto-assigned to the end of the category when omitted.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create(int formId, int categoryId, [FromBody] FormQuestionRequestDto request)
        {
            var userId = UserContextResolver.GetUserId(HttpContext);
            if (userId is null)
                return Unauthorized(new { message = "Sign in to create a question." });

            return await _service.CreateAsync(formId, categoryId, request, userId.Value);
        }

        /// <summary>
        /// Update a question's text, points or display order.
        /// </summary>
        [HttpPut("{questionId:int}")]
        public async Task<ActionResult> Update(int formId, int categoryId, int questionId, [FromBody] FormQuestionRequestDto request)
        {
            return await _service.UpdateAsync(formId, categoryId, questionId, request);
        }

        /// <summary>
        /// Delete a question from a form category.
        /// </summary>
        [HttpDelete("{questionId:int}")]
        public async Task<ActionResult> Delete(int formId, int categoryId, int questionId)
        {
            return await _service.DeleteAsync(formId, categoryId, questionId);
        }
    }
}
