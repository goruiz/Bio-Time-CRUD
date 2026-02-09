using BioTime.DTOs;
using BioTime.DTOs.Employees;
using BioTime.Services.Employees;
using Microsoft.AspNetCore.Mvc;

namespace BioTime.Controllers.Employees;

[ApiController]
[Route("api/[controller]")]
public class BioTimeController : ControllerBase
{
    private readonly IBioTimeService _bioTimeService;
    private readonly ILogger<BioTimeController> _logger;

    public BioTimeController(IBioTimeService bioTimeService, ILogger<BioTimeController> logger)
    {
        _bioTimeService = bioTimeService;
        _logger = logger;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _bioTimeService.GetEmployeesAsync(page, pageSize);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al comunicarse con BioTime.");
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpGet("employees/{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        try
        {
            var result = await _bioTimeService.GetEmployeeByIdAsync(id);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al obtener empleado {Id} de BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto employee)
    {
        try
        {
            var result = await _bioTimeService.CreateEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployeeById), new { id = result.Id }, result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al crear empleado en BioTime.");
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpPut("employees/{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto employee)
    {
        try
        {
            var result = await _bioTimeService.UpdateEmployeeAsync(id, employee);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al actualizar empleado {Id} en BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpDelete("employees/{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        try
        {
            await _bioTimeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al eliminar empleado {Id} en BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }
}
