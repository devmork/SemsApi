using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Facility;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;

namespace SemsApi.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateFacilityDto> _createValidator;
        private readonly IValidator<UpdateFacilityDto> _updateValidator;
        public FacilityService(
            ApplicationDbContext context,
            IValidator<CreateFacilityDto> createValidator,
            IValidator<UpdateFacilityDto> updateValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ActionResult<List<FacilityDto>>> GetAllAsync(
            string? search = null,
            bool? isActive = null,
            CancellationToken ct = default)
        {
            var query = _context.Facilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(f => f.FacilityName!.Contains(term));
            }

            if (isActive.HasValue) query = query.Where(f => f.IsActive == isActive.Value);

            var facility = await query
                .OrderBy(f => f.FacilityName)
                .ProjectToType<FacilityDto>()
                .ToListAsync(ct);

            return new OkObjectResult(facility);
        }

        public async Task<ActionResult<FacilityDto>> GetByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            var facility = await _context.Facilities
                .AsNoTracking()
                .Where(f => f.FacilityId == id)
                .ProjectToType<FacilityDto>()
                .FirstOrDefaultAsync(ct);

            if (facility is null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(facility);
        }

        public async Task<ActionResult<CreateFacilityDto>> CreateAsync(
            CreateFacilityDto dto,
            CancellationToken ct = default)
        {

            var validationResults = await _createValidator.ValidateAsync(dto, ct);
            if (!validationResults.IsValid)
            {
                return new BadRequestObjectResult(validationResults.ToValidationProblemDetails());
            }

            var name = dto.FacilityName!.Trim();
            if (await _context.Facilities.AnyAsync(f => f.FacilityName!.ToLower() == name.ToLower(), ct))
            {
                return new ConflictResult();
            }

            var facility = dto.Adapt<Facility>();
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync(ct);

            return new CreatedResult();
        }

        public async Task<ActionResult<UpdateFacilityDto>> UpdateAsync(
            int id, 
            UpdateFacilityDto dto, CancellationToken ct = default)
        {
            var validationResults = await _updateValidator.ValidateAsync(dto, ct);
            if (!validationResults.IsValid)
            {
                return new BadRequestObjectResult(validationResults.ToValidationProblemDetails());
            }

            var facility = await _context.Facilities
                .ProjectToType<Facility>()
                .FirstOrDefaultAsync(f => f.FacilityId == id, ct);
            if (facility is null)
            {
                return new NotFoundResult();
            }

            var facilityName = dto.FacilityName!.Trim();
            if (await _context.Facilities.AnyAsync(f => f.FacilityId != id && f.FacilityName!.ToLower() == facilityName.ToLower(), ct))
            {
                return new ConflictResult();
            }

            facility.FacilityName = facilityName;
            if (dto.IsActive.HasValue) facility.IsActive = dto.IsActive.Value;
            facility.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var count = await _context.FacilityEvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s => s.FacilityId == id, ct);

            return new OkObjectResult(facility);
        }

        public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var facility = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == id, ct);
            if (facility is null)
            {
                return new NotFoundResult();
            }

            var hasSubmissions = await _context.FacilityEvaluationSubmissions
                .AnyAsync(s => s.FacilityId == id, ct);
            if (hasSubmissions)
                return new ConflictResult();

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync(ct);
            return new NoContentResult();
        }
    }
}