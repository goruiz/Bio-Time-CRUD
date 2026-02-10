using BioTime.DTOs;
using BioTime.DTOs.Devices;

namespace BioTime.Services.Devices;

public interface IDeviceService
{
    Task<PaginatedResponse<TerminalDto>> GetTerminalsAsync(int page = 1, int pageSize = 10);
    Task<TerminalDto> GetTerminalByIdAsync(int id);
    Task<List<SyncResultDto>> SyncAllTerminalsAsync();
    Task<SyncResultDto> SyncTerminalBySnAsync(string sn);
}
