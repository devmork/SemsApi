using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationType;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/evaluation/types")]
    public class EvaluationTypeController : ControllerBase
    {
        private readonly IEvaluationTypeService _service;

        public EvaluationTypeController(IEvaluationTypeService service)
        {
            _service = service;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        [HttpGet]
        public async Task<ActionResult<List<EvaluationTypeResponseDto>>> GetAll(
            [FromQuery] string? search = null)
        {
            return await _service.GetAllAsync(search);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EvaluationTypeResponseDto>> GetById(int id)
        {
            return await _service.GetByIdAsync(id);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EvaluationTypeResponseDto>> Create(
            [FromBody] EvaluationTypeRequestDto request)
        {
            return await _service.CreateAsync(request);
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EvaluationTypeResponseDto>> Update(
            int id,
            [FromBody] EvaluationTypeRequestDto request)
        {
            return await _service.UpdateAsync(id, request);
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await _service.DeleteAsync(id);
        }
    }
}