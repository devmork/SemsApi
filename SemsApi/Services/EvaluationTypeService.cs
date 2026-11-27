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

        public EvaluationTypeService(ApplicationDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<List<EvaluationTypeDto>> GetAllAsync(string? search = null)
        {
            var query = _appDbContext.EvaluationTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(t => t.EvaluationTypeName!.Contains(value));
            }

            var getEvaluationTypes = await query
                .OrderBy(t => t.EvaluationTypeId)
                .ProjectToType<EvaluationTypeDto>()
                .ToListAsync();

            return getEvaluationTypes;
        }

        public async Task<EvaluationTypeDto?> GetByIdAsync(int id)
        {
            var evaluationType = await _appDbContext.EvaluationTypes
                    .AsNoTracking()
                    .ProjectToType<EvaluationTypeDto>()
                    .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            return evaluationType;
        }

        public async Task<CreateEvaluationTypeDto> CreateAsync(CreateEvaluationTypeDto dto)
        {
            var evaluationTypeName = dto.EvaluationTypeName!.Trim();
            var duplicateExists = await _appDbContext.EvaluationTypes
                                            .AnyAsync(t => t.EvaluationTypeName!.ToLower() == evaluationTypeName.ToLower());

            if (duplicateExists)
            {
                throw new Exception($"Evaluation type with name '{evaluationTypeName}' already exists.");
            }

            var evaluationType = dto.Adapt<EvaluationType>();
            _appDbContext.EvaluationTypes.Add(evaluationType);
            await _appDbContext.SaveChangesAsync();

            return evaluationType.Adapt<CreateEvaluationTypeDto>();
        }

        public async Task<UpdateEvaluationTypeDto> UpdateAsync(int id, UpdateEvaluationTypeDto dto)
        {
            var evaluationType = await _appDbContext.EvaluationTypes.FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            var evaluationTypeName = dto.EvaluationTypeName!.Trim();
            var duplicateExists = await _appDbContext.EvaluationTypes
                               .AnyAsync(t => t.EvaluationTypeId != id &&
                                         t.EvaluationTypeName!.ToLower() == evaluationTypeName.ToLower());

            if (duplicateExists)
            {
                throw new Exception($"Evaluation type with name '{evaluationTypeName}' already exists.");
            }

            evaluationType!.EvaluationTypeName = evaluationTypeName;
            await _appDbContext.SaveChangesAsync();
            return evaluationType!.Adapt<UpdateEvaluationTypeDto>();
        }

        public async Task<EvaluationTypeDto> DeleteAsync(int id)
        {
            var evaluationType = await _appDbContext.EvaluationTypes
                .FirstOrDefaultAsync(t => t.EvaluationTypeId == id);

            var inUse =
                   await _appDbContext.TeacherEvaluationCategories.AnyAsync(c => c.EvaluationTypeId == id)
                || await _appDbContext.Forms.AnyAsync(f => f.EvaluationTypeId == id)
                || await _appDbContext.TeacherEvaluationComments.AnyAsync(s => s.EvaluationTypeId == id);

            if (inUse)
            {
                throw new Exception($"Evaluation type with ID '{id}' is in use and cannot be deleted.");
            }

            _appDbContext.EvaluationTypes.Remove(evaluationType!);
            await _appDbContext.SaveChangesAsync();
            return evaluationType!.Adapt<EvaluationTypeDto>();
        }
    }
}