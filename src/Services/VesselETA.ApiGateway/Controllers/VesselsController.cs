using Microsoft.AspNetCore.Mvc;
using VesselETA.ApiGateway.Services;

namespace VesselETA.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VesselsController : ControllerBase
{
    private readonly IVesselService _vesselService;
    private readonly ILogger<VesselsController> _logger;

    public VesselsController(IVesselService vesselService, ILogger<VesselsController> logger)
    {
        _vesselService = vesselService;
        _logger = logger;
    }

    [HttpGet("{mmsi}/eta")]
    public async Task<IActionResult> GetVesselEta(string mmsi)
    {
        try
        {
            var eta = await _vesselService.GetLatestEtaAsync(mmsi);
            if (eta == null)
            {
                return NotFound($"No ETA found for vessel {mmsi}");
            }

            return Ok(eta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ETA for vessel {Mmsi}", mmsi);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{mmsi}/history")]
    public async Task<IActionResult> GetVesselEtaHistory(string mmsi, [FromQuery] int hours = 24)
    {
        try
        {
            var history = await _vesselService.GetEtaHistoryAsync(mmsi, hours);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ETA history for vessel {Mmsi}", mmsi);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVessels()
    {
        try
        {
            var vessels = await _vesselService.GetAllVesselsAsync();
            return Ok(vessels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vessels");
            return StatusCode(500, "Internal server error");
        }
    }
}