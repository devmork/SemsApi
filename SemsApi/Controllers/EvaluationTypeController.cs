using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.EvaluationType;
using SemsApi.Extensions;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/evaluation/types")]
    public class EvaluationTypeController : ControllerBase
    {
        private readonly IEvaluationTypeService _evaluationService;

        public EvaluationTypeController(IEvaluationTypeService service)
        {
            _evaluationService = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<EvaluationTypeDto>>> GetAllEvaluationType([FromQuery] string? search = null)
        {
            var evaluationTypes = await _evaluationService.GetAllAsync(search);
            return Ok(evaluationTypes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EvaluationTypeDto>> GetEvaluationTypeById(int id)
        {
            var evaluationType = await _evaluationService.GetByIdAsync(id);

            if (evaluationType == null)
            {
                return NotFound();
            }

            return Ok(evaluationType);
        }

        [HttpPost]
        public async Task<ActionResult<CreateEvaluationTypeDto>> CreateEvaluationType(
            [FromBody] CreateEvaluationTypeDto dto,
            [FromServices] IValidator<CreateEvaluationTypeDto> validator)
        {
            var validationResults = await validator.ValidateAsync(dto);
            if (!validationResults.IsValid)
            {
                return ValidationProblem(validationResults.ToModelStateDictionary());
            }

            await _evaluationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetEvaluationTypeById), new { id = dto.EvaluationTypeName });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UpdateEvaluationTypeDto>> UpdateEvaluationType(
            int id, 
            [FromBody] UpdateEvaluationTypeDto dto,
            [FromServices] IValidator<UpdateEvaluationTypeDto> validator)
        {
            var validationResults = await validator.ValidateAsync(dto);
            if (!validationResults.IsValid)
            {
                return ValidationProblem(validationResults.ToModelStateDictionary());
            }

            var existingEvaluationType = await _evaluationService.GetByIdAsync(id);
            if (existingEvaluationType == null)
            {
                return NotFound();
            }

            var result = await _evaluationService.UpdateAsync(id, dto);
            return Ok(result.Data);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteEvaluationTypeById(int id)
        {
            var existingEvaluationType = await _evaluationService.GetByIdAsync(id);

            if (existingEvaluationType == null)
            {
                return NotFound();
            }

            await _evaluationService.DeleteAsync(id);
            return Ok();
        }
    }
}
