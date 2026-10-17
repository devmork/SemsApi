using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/evaluation/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll(
            [FromQuery] string? evalType = null,
            [FromQuery] int? formId = null)
        {
            return await _service.GetAllAsync(evalType, formId);
        }

        [HttpGet("{recno:int}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(int recno)
        {
            return await _service.GetByIdAsync(recno);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CategoryRequestDto request)
        {
            return await _service.CreateAsync(request);
        }

        [HttpPut("{recno:int}")]
        public async Task<ActionResult> Update(int recno, [FromBody] CategoryRequestDto request)
        {
            return await _service.UpdateAsync(recno, request);
        }

        [HttpDelete("{recno:int}")]
        public async Task<ActionResult> Delete(int recno)
        {
            return await _service.DeleteAsync(recno);
        }
    }
}