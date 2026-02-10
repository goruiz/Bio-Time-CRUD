using BioTime.Services.Devices;
using Microsoft.AspNetCore.Mvc;

namespace BioTime.Controllers.Devices;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    [HttpGet("terminals")]
    public async Task<IActionResult> GetTerminals([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _deviceService.GetTerminalsAsync(page, pageSize);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al obtener terminales de BioTime.");
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpGet("terminals/{id}")]
    public async Task<IActionResult> GetTerminalById(int id)
    {
        try
        {
            var result = await _deviceService.GetTerminalByIdAsync(id);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al obtener terminal {Id} de BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncAllTerminals()
    {
        try
        {
            var results = await _deviceService.SyncAllTerminalsAsync();
            return Ok(new { message = "Sincronización completada.", results });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al sincronizar terminales.");
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpPost("sync/{sn}")]
    public async Task<IActionResult> SyncTerminal(string sn)
    {
        try
        {
            var result = await _deviceService.SyncTerminalBySnAsync(sn);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al sincronizar terminal {Sn}.", sn);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }
}
