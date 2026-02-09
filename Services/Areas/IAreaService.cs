using BioTime.DTOs;
using BioTime.DTOs.Areas;

namespace BioTime.Services.Areas;

public interface IAreaService
{
    Task<PaginatedResponse<AreaDto>> GetAreasAsync(int page = 1, int pageSize = 10);
    Task<AreaDto> GetAreaByIdAsync(int id);
    Task<AreaDto> CreateAreaAsync(CreateAreaDto area);
    Task<AreaDto> UpdateAreaAsync(int id, UpdateAreaDto area);
    Task DeleteAreaAsync(int id);
}
