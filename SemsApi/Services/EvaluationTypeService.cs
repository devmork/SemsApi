using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.EvaluationType;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Models.Eval;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    public class EvaluationTypeService : IEvaluationTypeService
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IValidator<CreateEvaluationTypeDto> _createValidator;
        private readonly IValidator<UpdateEvaluationTypeDto> _updateValidator;


        public EvaluationTypeService(
            ApplicationDbContext appDbContext,
            IValidator<CreateEvaluationTypeDto> createValidator,
            IValidator<UpdateEvaluationTypeDto> updateValidator)
        {
            _appDbContext = appDbContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }
        public async Task<ActionResult<List<GetEvaluationTypeDto>>> GetAllAsync(string? search = null)
        {
            var query = _appDbContext.EvaluationTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(t => t.EvaluationTypeName!.Contains(value));
            }

            var getEvaluationTypes = await query
                .OrderBy(t => t.EvaluationTypeId)
                .ProjectToType<GetEvaluationTypeDto>()
                .ToListAsync();

            return new OkObjectResult(getEvaluationTypes);
        }

        public async Task<ActionResult<GetEvaluationTypeDto>> GetByIdAsync(int id)
        {
            var evaluationType = await _appDbContext.EvaluationTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            if (evaluationType is null)
            {
                return new NotFoundObjectResult(new { message = "Evaluation type not found." });
            }

            return new OkObjectResult(evaluationType.Adapt<GetEvaluationTypeDto>());
        }

        public async Task<ActionResult<CreateEvaluationTypeDto>> CreateAsync(CreateEvaluationTypeDto dto)
        {
            var validationResults = await _createValidator.ValidateAsync(dto);
            if (!validationResults.IsValid)
            {
                return new BadRequestObjectResult(validationResults.ToValidationProblemDetails());
            }

            var name = dto.EvaluationTypeName!.Trim();
            var duplicateExists = await _appDbContext.EvaluationTypes.AnyAsync(t => t.EvaluationTypeName!.ToLower() == name.ToLower());

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation type with the same name already exists."
                });
            }

            var evaluationType = dto.Adapt<EvaluationType>();
            _appDbContext.EvaluationTypes.Add(evaluationType);

            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (Utilities.DbUpdateExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                return new ConflictObjectResult(new { message = "An evaluation type with the same name already exists." });
            }

            return new CreatedResult();
        }

        public async Task<ActionResult<UpdateEvaluationTypeDto>> UpdateAsync(int id, UpdateEvaluationTypeDto dto)
        {
            var evaluationType = await _appDbContext.EvaluationTypes.FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            if (evaluationType is null)
            {
                return new NotFoundObjectResult(new { message = "Evaluation type not found." });
            }

            var validationResults = await _updateValidator.ValidateAsync(dto);
            if (!validationResults.IsValid)
            {
                return new BadRequestObjectResult(validationResults.ToValidationProblemDetails());
            }

            var evaluationTypeName = dto.EvaluationTypeName!.Trim();
            var duplicateExists = await _appDbContext.EvaluationTypes
                               .AnyAsync(t => t.EvaluationTypeId != id && 
                                         t.EvaluationTypeName!.ToLower() == evaluationTypeName.ToLower());

            if (duplicateExists)
            {
                return new ConflictObjectResult(new
                {
                    message = "An evaluation type with the same name already exists."
                });
            }

            evaluationType.EvaluationTypeName = evaluationTypeName;

            await _appDbContext.SaveChangesAsync();

            return new OkObjectResult(evaluationType.Adapt<UpdateEvaluationTypeDto>());
        }

        public async Task<ActionResult> DeleteAsync(int id)
        {
            var evaluationType = await _appDbContext.EvaluationTypes
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            if (evaluationType is null)
            {
                return new NotFoundObjectResult(new { message = "Evaluation type not found." });
            }

            var inUse =
                   await _appDbContext.TeacherEvaluationCategories.AnyAsync(c => c.EvaluationTypeId == id)
                || await _appDbContext.Forms.AnyAsync(f => f.EvaluationTypeId == id)
                || await _appDbContext.TeacherEvaluationComments.AnyAsync(s => s.EvaluationTypeId == id);

            if (inUse)
            {
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete this evaluation type because categories, forms or comments are using it."
                });
            }

            _appDbContext.EvaluationTypes.Remove(evaluationType);
            await _appDbContext.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}