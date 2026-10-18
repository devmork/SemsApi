using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    /// <summary>
    /// Dedicated controller for TeacherEvaluationCategory CRUD operations.
    /// All logic lives in ICategoryService; this controller only handles HTTP concerns.
    /// A category belongs to one evaluation type, and its CatNo is unique across all evaluation types.
    /// </summary>
    [ApiController]
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
        /// Get a single category by CatNo.
        /// </summary>
        [HttpGet("{catNo:int}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(int catNo)
        {
            return await _service.GetByIdAsync(catNo);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "QA")]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CategoryRequestDto request)
        {
            return await _service.CreateAsync(request);
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "QA")]
        [HttpPut("{catNo:int}")]
        public async Task<ActionResult> Update(int catNo, [FromBody] CategoryRequestDto request)
        {
            return await _service.UpdateAsync(catNo, request);
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "QA")]
        [HttpDelete("{catNo:int}")]
        public async Task<ActionResult> Delete(int catNo)
        {
            return await _service.DeleteAsync(catNo);
        }
    }
}