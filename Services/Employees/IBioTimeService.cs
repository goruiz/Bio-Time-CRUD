using BioTime.DTOs;
using BioTime.DTOs.Employees;

namespace BioTime.Services.Employees;

public interface IBioTimeService
{
    Task<PaginatedResponse<EmployeeDto>> GetEmployeesAsync(int page = 1, int pageSize = 10);
    Task<EmployeeDto> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto employee);
    Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto employee);
    Task DeleteEmployeeAsync(int id);
}
