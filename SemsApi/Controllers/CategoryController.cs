using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    /// <summary>
    /// Dedicated controller for TeacherEvaluationCategory CRUD operations.
    /// All logic lives in ICategoryService; this controller only handles HTTP concerns.
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
        /// List all categories. Optionally filter by evalType.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll(
            [FromQuery] string? evalType = null)
        {
            return await _service.GetAllAsync(evalType);
        }

        /// <summary>
        /// Get a single category by Recno.
        /// </summary>
        [HttpGet("{recno:int}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(int recno)
        {
            return await _service.GetByIdAsync(recno);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CategoryRequestDto request)
        {
            return await _service.CreateAsync(request);
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "Admin")]
        [HttpPut("{recno:int}")]
        public async Task<ActionResult> Update(int recno, [FromBody] CategoryRequestDto request)
        {
            return await _service.UpdateAsync(recno, request);
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "Admin")]
        [HttpDelete("{recno:int}")]
        public async Task<ActionResult> Delete(int recno)
        {
            return await _service.DeleteAsync(recno);
        }
    }
}