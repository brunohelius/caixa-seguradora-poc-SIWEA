using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Service for migration dashboard operations
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets dashboard overview with key metrics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard overview data</returns>
    Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user story component status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Component status list</returns>
    Task<IEnumerable<ComponentStatusDto>> GetComponentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets performance metrics
    /// </summary>
    /// <param name="days">Number of days to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics</returns>
    Task<IEnumerable<PerformanceMetricDto>> GetPerformanceMetricsAsync(
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent activities
    /// </summary>
    /// <param name="limit">Number of activities to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent activities</returns>
    Task<IEnumerable<ActivityDto>> GetRecentActivitiesAsync(
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets system health indicators
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System health data</returns>
    Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default);
}
