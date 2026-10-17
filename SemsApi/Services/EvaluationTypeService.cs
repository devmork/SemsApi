using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.EvaluationType;
using SemsApi.Interfaces;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    public class EvaluationTypeService : IEvaluationTypeService
    {
        private readonly ApplicationDbContext _context;

        public EvaluationTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        public async Task<ActionResult<List<EvaluationTypeResponseDto>>> GetAllAsync(string? search = null)
        {
            var query = _context.EvaluationTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t => t.EvaluationTypeName.Contains(term));
            }

            var data = await query
                .OrderBy(t => t.EvaluationTypeId)
                .Select(t => new EvaluationTypeResponseDto
                {
                    EvaluationTypeId = t.EvaluationTypeId,
                    EvaluationTypeName = t.EvaluationTypeName
                })
                .ToListAsync();

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<EvaluationTypeResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _context.EvaluationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            if (entity is null)
                return new NotFoundObjectResult(new { message = "Evaluation type not found." });

            return new OkObjectResult(EvaluationTypeHelper.MapToResponse(entity));
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult<EvaluationTypeResponseDto>> CreateAsync(EvaluationTypeRequestDto request)
        {
            var validationError = EvaluationTypeHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var name = request.EvaluationTypeName!.Trim();

            var duplicateExists = await _context.EvaluationTypes
                .AnyAsync(t => t.EvaluationTypeName.ToLower() == name.ToLower());

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation type with the same name already exists."
                });
            }

            var entity = new EvaluationType
            {
                EvaluationTypeName = name
            };

            _context.EvaluationTypes.Add(entity);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(
                actionName: nameof(GetByIdAsync),
                controllerName: null,
                routeValues: new { id = entity.EvaluationTypeId },
                value: new
                {
                    message = "Evaluation type created successfully.",
                    data = EvaluationTypeHelper.MapToResponse(entity)
                });
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        public async Task<ActionResult<EvaluationTypeResponseDto>> UpdateAsync(int id, EvaluationTypeRequestDto request)
        {
            var entity = await _context.EvaluationTypes
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            if (entity is null)
                return new NotFoundObjectResult(new { message = "Evaluation type not found." });

            var validationError = EvaluationTypeHelper.Validate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var name = request.EvaluationTypeName!.Trim();

            var duplicateExists = await _context.EvaluationTypes
                .AnyAsync(t => t.EvaluationTypeId != id
                            && t.EvaluationTypeName.ToLower() == name.ToLower());

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation type with the same name already exists."
                });
            }

            entity.EvaluationTypeName = name;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Evaluation type updated successfully.",
                data = EvaluationTypeHelper.MapToResponse(entity)
            });
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        public async Task<ActionResult> DeleteAsync(int id)
        {
            var entity = await _context.EvaluationTypes
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            if (entity is null)
                return new NotFoundObjectResult(new { message = "Evaluation type not found." });

            _context.EvaluationTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}