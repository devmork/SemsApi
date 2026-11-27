using SemsApi.DTO.EvaluationType;
using SemsApi.Utilities;

namespace SemsApi.Interfaces
{
    public interface IEvaluationTypeService
    {
        Task<List<EvaluationTypeDto>> GetAllAsync(string? search = null);
        Task<EvaluationTypeDto?> GetByIdAsync(int id);
        Task<CreateEvaluationTypeDto> CreateAsync(CreateEvaluationTypeDto dto);
        Task<UpdateEvaluationTypeDto> UpdateAsync(int id, UpdateEvaluationTypeDto dto);
        Task<EvaluationTypeDto> DeleteAsync(int id);
    }
}
