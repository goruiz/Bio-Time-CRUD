using BioTime.DTOs.Areas;
using BioTime.DTOs.Devices;
using BioTime.Services.Areas;
using BioTime.Services.Devices;
using Microsoft.AspNetCore.Mvc;

namespace BioTime.Controllers.Areas;

[ApiController]
[Route("api/[controller]")]
public class AreaController : ControllerBase
{
    private readonly IAreaService _areaService;
    private readonly IDeviceService _deviceService;
    private readonly ILogger<AreaController> _logger;

    public AreaController(
        IAreaService areaService,
        IDeviceService deviceService,
        ILogger<AreaController> logger)
    {
        _areaService = areaService;
        _deviceService = deviceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAreas([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _areaService.GetAreasAsync(page, pageSize);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al comunicarse con BioTime.");
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAreaById(int id)
    {
        try
        {
            var result = await _areaService.GetAreaByIdAsync(id);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al obtener área {Id} de BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateArea([FromBody] CreateAreaDto area)
    {
        try
        {
            var result = await _areaService.CreateAreaAsync(area);

            var syncResults = await SyncDevicesAsync();

            return CreatedAtAction(nameof(GetAreaById), new { id = result.Id },
                new { data = result, sync = syncResults });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al crear área en BioTime.");
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArea(int id, [FromBody] UpdateAreaDto area)
    {
        try
        {
            var result = await _areaService.UpdateAreaAsync(id, area);

            var syncResults = await SyncDevicesAsync();

            return Ok(new { data = result, sync = syncResults });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al actualizar área {Id} en BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArea(int id)
    {
        try
        {
            await _areaService.DeleteAreaAsync(id);

            await SyncDevicesAsync();

            return NoContent();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al eliminar área {Id} en BioTime.", id);
            return StatusCode(502, new { error = "Error al comunicarse con BioTime.", detail = ex.Message });
        }
    }

    private async Task<List<SyncResultDto>?> SyncDevicesAsync()
    {
        try
        {
            return await _deviceService.SyncAllTerminalsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CRUD exitoso pero falló la sincronización con dispositivos.");
            return null;
        }
    }
}
