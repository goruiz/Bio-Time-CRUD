using BioTime.Services;
using Microsoft.AspNetCore.Mvc;

namespace BioTime.Controllers;

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
}
