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
    //[Authorize]
    [Route("api/evaluation/types")]
    public class EvaluationTypeController : ControllerBase
    {
        private readonly IEvaluationTypeService _evaluationService;

        public EvaluationTypeController(IEvaluationTypeService service)
        {
            _evaluationService = service;
        }

        /// <summary>
        /// Get all evaluation types
        /// </summary>
        /// <param name="search"></param>
        /// <returns>An array of evaluation types</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EvaluationTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<EvaluationTypeDto>>> GetAllEvaluationType([FromQuery] string? search = null)
        {
            var evaluationTypes = await _evaluationService.GetAllAsync(search);
            return Ok(evaluationTypes);
        }

        /// <summary>
        /// Get evaluation type by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The evaluation type if found, otherwise a 404 Not Found response</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EvaluationTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EvaluationTypeDto>> GetEvaluationTypeById(int id)
        {
            var evaluationType = await _evaluationService.GetByIdAsync(id);

            if (evaluationType == null)
            {
                return NotFound();
            }

            return Ok(evaluationType);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateEvaluationTypeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateEvaluationTypeDto>> CreateEvaluationType([FromBody] CreateEvaluationTypeDto dto)
        {
            var created = await _evaluationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetEvaluationTypeById), new { id = created.EvaluationTypeId }, created);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UpdateEvaluationTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UpdateEvaluationTypeDto>> UpdateEvaluationType(int id, [FromBody] UpdateEvaluationTypeDto dto)
        {            
            var existingEvaluationType = await _evaluationService.GetByIdAsync(id);
            if (existingEvaluationType == null)
            {
                return NotFound();
            }

            var result = await _evaluationService.UpdateAsync(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(EvaluationTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EvaluationTypeDto>> DeleteEvaluationTypeById(int id)
        {
            var existingEvaluationType = await _evaluationService.GetByIdAsync(id);

            if (existingEvaluationType == null)
            {
                return NotFound();
            }

            await _evaluationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
