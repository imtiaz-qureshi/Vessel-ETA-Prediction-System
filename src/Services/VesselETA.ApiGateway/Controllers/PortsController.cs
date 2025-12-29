using Microsoft.AspNetCore.Mvc;
using VesselETA.ApiGateway.Services;

namespace VesselETA.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortsController : ControllerBase
{
    private readonly IPortService _portService;
    private readonly ILogger<PortsController> _logger;

    public PortsController(IPortService portService, ILogger<PortsController> logger)
    {
        _portService = portService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPorts()
    {
        try
        {
            var ports = await _portService.GetAllPortsAsync();
            return Ok(ports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all ports");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{portCode}")]
    public async Task<IActionResult> GetPort(string portCode)
    {
        try
        {
            var port = await _portService.GetPortByCodeAsync(portCode);
            if (port == null)
            {
                return NotFound($"Port with code '{portCode}' not found");
            }

            return Ok(port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving port {PortCode}", portCode);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{portCode}/vessels")]
    public async Task<IActionResult> GetVesselsAtPort(string portCode)
    {
        try
        {
            var vessels = await _portService.GetVesselsAtPortAsync(portCode);
            return Ok(vessels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vessels for port {PortCode}", portCode);
            return StatusCode(500, "Internal server error");
        }
    }
}