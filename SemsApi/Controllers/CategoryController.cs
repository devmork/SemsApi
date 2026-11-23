using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    /// <summary>
    /// Dedicated controller for EvaluationCategory CRUD operations.
    /// All logic lives in ICategoryService; this controller only handles HTTP concerns.
    /// A category belongs to one evaluation type, and its CategoryId is unique across all evaluation types.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/evaluation/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        /// <summary>
        /// List all categories. Optionally filter by evaluationTypeId.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll(
            [FromQuery] int? evaluationTypeId = null)
        {
            return await _service.GetAllAsync(evaluationTypeId);
        }

        /// <summary>
        /// Get a single category by CategoryId.
        /// </summary>
        [HttpGet("{categoryId:int}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(int categoryId)
        {
            return await _service.GetByIdAsync(categoryId);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CategoryRequestDto request)
        {
            return await _service.CreateAsync(request);
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        [HttpPut("{categoryId:int}")]
        public async Task<ActionResult> Update(int categoryId, [FromBody] CategoryRequestDto request)
        {
            return await _service.UpdateAsync(categoryId, request);
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        [HttpDelete("{categoryId:int}")]
        public async Task<ActionResult> Delete(int categoryId)
        {
            return await _service.DeleteAsync(categoryId);
        }
    }
}
