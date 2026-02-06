using BioTime.DTOs.BioTime;

namespace BioTime.Services;

public interface IBioTimeService
{
    Task<PaginatedResponse<EmployeeDto>> GetEmployeesAsync(int page = 1, int pageSize = 10);
}
