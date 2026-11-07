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
        public FacilityService(ApplicationDbContext context) { _context = context; }

        public async Task<ActionResult<List<FacilityDto>>> GetAllAsync(
            string? search = null, bool? isActive = null, CancellationToken ct = default)
        {
            var query = _context.Facilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(f => f.FacilityName.Contains(term));
            }
            if (isActive.HasValue) query = query.Where(f => f.IsActive == isActive.Value);

            var data = await query
                .OrderBy(f => f.FacilityName)
                .Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    EvaluationCount = f.EvaluationSubmissions.Count
                })
                .ToListAsync(ct);

            return new OkObjectResult(data);
        }

        public async Task<ActionResult<FacilityDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dto = await _context.Facilities.AsNoTracking()
                .Where(f => f.FacilityId == id)
                .Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    EvaluationCount = f.EvaluationSubmissions.Count
                })
                .FirstOrDefaultAsync(ct);

            if (dto is null)
                return new NotFoundObjectResult(new { message = "Facility not found." });

            return new OkObjectResult(dto);
        }

        public async Task<ActionResult<FacilityDto>> CreateAsync(
            CreateFacilityRequest request, CancellationToken ct = default)
        {
            var validationError = FacilityHelper.ValidateCreate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var name = request.FacilityName.Trim();
            if (await _context.Facilities.AnyAsync(f => f.FacilityName.ToLower() == name.ToLower(), ct))
                return new ConflictObjectResult(new { message = "A facility with this name already exists." });

            var entity = new Facility
            {
                FacilityName = name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Facilities.Add(entity);
            await _context.SaveChangesAsync(ct);

            return new CreatedResult(
                $"/api/facilities/{entity.FacilityId}",
                FacilityHelper.MapToDto(entity, 0));
        }

        public async Task<ActionResult<FacilityDto>> UpdateAsync(
            int id, UpdateFacilityRequest request, CancellationToken ct = default)
        {
            var validationError = FacilityHelper.ValidateUpdate(request);
            if (validationError is not null)
                return new BadRequestObjectResult(new { message = validationError });

            var entity = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == id, ct);
            if (entity is null)
                return new NotFoundObjectResult(new { message = "Facility not found." });

            var name = request.FacilityName.Trim();
            if (await _context.Facilities.AnyAsync(
                    f => f.FacilityId != id && f.FacilityName.ToLower() == name.ToLower(), ct))
                return new ConflictObjectResult(new { message = "A facility with this name already exists." });

            entity.FacilityName = name;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var count = await _context.FacilityEvaluationSubmissions
                .AsNoTracking().CountAsync(s => s.FacilityId == id, ct);

            return new OkObjectResult(FacilityHelper.MapToDto(entity, count));
        }

        public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == id, ct);
            if (entity is null)
                return new NotFoundObjectResult(new { message = "Facility not found." });

            var hasSubmissions = await _context.FacilityEvaluationSubmissions
                .AnyAsync(s => s.FacilityId == id, ct);
            if (hasSubmissions)
                return new ConflictObjectResult(new
                {
                    message = "Cannot delete: facility has evaluation submissions. Deactivate it instead."
                });

            _context.Facilities.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return new NoContentResult();
        }
    }
}