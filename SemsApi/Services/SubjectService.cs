using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.DTO.Subject;
using SemsApi.DTO.Subjects;
using SemsApi.Interfaces;
using SemsApi.Models;

namespace SemsApi.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ApplicationDbContext _context;
        public SubjectService(ApplicationDbContext context) { _context = context; }

        public async Task<ActionResult<List<SubjectDto>>> GetAllAsync(string? search = null, CancellationToken ct = default)
        {
            var query = _context.Subjects.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var t = search.Trim();
                query = query.Where(s => s.Code.Contains(t) || s.Description.Contains(t));
            }
            var data = await query
                .OrderBy(s => s.Code)
                .Select(s => new SubjectDto { SubjectId = s.SubjectId, Code = s.Code, Description = s.Description, IsActive = s.IsActive })
                .ToListAsync(ct);
            return new OkObjectResult(data);
        }

        public async Task<ActionResult<SubjectDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.SubjectId == id, ct);
            if (entity is null) return new NotFoundObjectResult(new { message = "Subject not found." });
            return new OkObjectResult(Map(entity));
        }

        public async Task<ActionResult<SubjectDto>> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Description))
                return new BadRequestObjectResult(new { message = "Code and Description are required." });

            var code = request.Code.Trim().ToUpperInvariant();
            if (await _context.Subjects.AnyAsync(s => s.Code == code, ct))
                return new ConflictObjectResult(new { message = "A subject with this Code already exists." });

            var entity = new Subject { Code = code, Description = request.Description.Trim() };
            _context.Subjects.Add(entity);
            await _context.SaveChangesAsync(ct);
            return new CreatedResult($"/api/subjects/{entity.SubjectId}", Map(entity));
        }

        public async Task<ActionResult<SubjectDto>> UpdateAsync(int id, UpdateSubjectRequest request, CancellationToken ct = default)
        {
            var entity = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectId == id, ct);
            if (entity is null) return new NotFoundObjectResult(new { message = "Subject not found." });

            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Description))
                return new BadRequestObjectResult(new { message = "Code and Description are required." });

            var code = request.Code.Trim().ToUpperInvariant();
            if (await _context.Subjects.AnyAsync(s => s.SubjectId != id && s.Code == code, ct))
                return new ConflictObjectResult(new { message = "A subject with this Code already exists." });

            entity.Code = code;
            entity.Description = request.Description.Trim();
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync(ct);
            return new OkObjectResult(Map(entity));
        }

        public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectId == id, ct);
            if (entity is null) return new NotFoundObjectResult(new { message = "Subject not found." });

            if (await _context.SubjectOfferings.AnyAsync(o => o.SubjectId == id, ct))
                return new ConflictObjectResult(new { message = "Cannot delete: subject still has offerings." });

            _context.Subjects.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return new NoContentResult();
        }

        private static SubjectDto Map(Subject s) => new()
        {
            SubjectId = s.SubjectId,
            Code = s.Code,
            Description = s.Description,
            IsActive = s.IsActive
        };
    }
}