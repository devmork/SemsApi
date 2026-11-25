using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationType;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/evaluation/types")]
    public class EvaluationTypeController : ControllerBase
    {
        private readonly IEvaluationTypeService _service;

        public EvaluationTypeController(IEvaluationTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<GetEvaluationTypeDto>>> GetAll([FromQuery] string? search = null)
        {
            return await _service.GetAllAsync(search);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetEvaluationTypeDto>> GetById(int id)
        {
            return await _service.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<ActionResult<CreateEvaluationTypeDto>> Create([FromBody] CreateEvaluationTypeDto dto)
        {
            return await _service.CreateAsync(dto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UpdateEvaluationTypeDto>> Update(int id, [FromBody] UpdateEvaluationTypeDto dto)
        {
            return await _service.UpdateAsync(id, dto);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await _service.DeleteAsync(id);
        }
    }
}
