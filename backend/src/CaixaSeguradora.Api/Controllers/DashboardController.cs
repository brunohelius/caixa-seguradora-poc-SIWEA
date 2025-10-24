using Microsoft.AspNetCore.Mvc;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Api.Controllers;

/// <summary>
/// Dashboard and monitoring endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard overview with key migration metrics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard overview data</returns>
    /// <response code="200">Overview retrieved successfully</response>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await _dashboardService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Gets user story component status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Component status list</returns>
    /// <response code="200">Components retrieved successfully</response>
    [HttpGet("components")]
    [ProducesResponseType(typeof(IEnumerable<ComponentStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComponentStatusDto>>> GetComponents(CancellationToken cancellationToken)
    {
        var components = await _dashboardService.GetComponentsAsync(cancellationToken);
        return Ok(components);
    }

    /// <summary>
    /// Gets performance metrics for specified time period
    /// </summary>
    /// <param name="days">Number of days to retrieve (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics</returns>
    /// <response code="200">Metrics retrieved successfully</response>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceMetricDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PerformanceMetricDto>>> GetPerformanceMetrics(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (days < 1 || days > 365)
        {
            return BadRequest(new { error = "Days parameter must be between 1 and 365" });
        }

        var metrics = await _dashboardService.GetPerformanceMetricsAsync(days, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Gets recent system activities
    /// </summary>
    /// <param name="limit">Number of activities to retrieve (default: 50, max: 200)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent activities</returns>
    /// <response code="200">Activities retrieved successfully</response>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(IEnumerable<ActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetRecentActivities(
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 200)
        {
            return BadRequest(new { error = "Limit parameter must be between 1 and 200" });
        }

        var activities = await _dashboardService.GetRecentActivitiesAsync(limit, cancellationToken);
        return Ok(activities);
    }

    /// <summary>
    /// Gets system health indicators
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System health data</returns>
    /// <response code="200">System health retrieved successfully</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(SystemHealthDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth(CancellationToken cancellationToken)
    {
        var health = await _dashboardService.GetSystemHealthAsync(cancellationToken);
        return Ok(health);
    }
}
