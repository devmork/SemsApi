using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Evaluation;
using SemsApi.Interfaces;

namespace SemsApi.Controllers
{
    [ApiController]
    [Route("api/evaluation/forms")]
    public class EvaluationFormController : ControllerBase
    {
        private readonly IEvaluationFormService _service;

        public EvaluationFormController(IEvaluationFormService service)
        {
            _service = service;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        /// <summary>
        /// Get all evaluation forms.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EvaluationFormDto>>> GetAll(
            [FromQuery] bool onlyActive = false)
        {
            return await _service.GetAllAsync(onlyActive);
        }

        /// <summary>
        /// Get a single form by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EvaluationFormDto>> GetById(int id)
        {
            return await _service.GetByIdAsync(id);
        }

        /// <summary>
        /// Get a single form by its Code (e.g. PRE-G3, CAFETERIA, LIBRARY).
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<EvaluationFormDto>> GetByCode(string code)
        {
            return await _service.GetByCodeAsync(code);
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Create a new Evaluation Form.
        /// TargetType must be either "SubjectTeacher" or "Facility".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] EvaluationFormRequestDto request)
        {
            return await _service.CreateAsync(request);
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Update an existing Evaluation Form.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] EvaluationFormRequestDto request)
        {
            return await _service.UpdateAsync(id, request);
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Soft delete (recommended) – sets IsActive = false.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> SoftDelete(int id)
        {
            return await _service.SoftDeleteAsync(id);
        }

        /// <summary>
        /// Hard delete – only allowed if the form has no categories.
        /// </summary>
        [HttpDelete("{id:int}/hard")]
        public async Task<ActionResult> HardDelete(int id)
        {
            return await _service.HardDeleteAsync(id);
        }
    }
}