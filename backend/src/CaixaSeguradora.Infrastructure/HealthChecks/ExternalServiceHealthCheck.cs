using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.HealthChecks;

/// <summary>
/// Health check for external validation services
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly IEnumerable<IExternalValidationService> _validationServices;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;

    public ExternalServiceHealthCheck(
        IEnumerable<IExternalValidationService> validationServices,
        ILogger<ExternalServiceHealthCheck> logger)
    {
        _validationServices = validationServices;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var serviceStatuses = new Dictionary<string, object>();
        var unhealthyServices = new List<string>();
        var degradedServices = new List<string>();

        try
        {
            // Check each external validation service
            var healthCheckTasks = _validationServices.Select(async service =>
            {
                try
                {
                    var healthStatus = await service.CheckHealthAsync(cancellationToken);

                    serviceStatuses[service.SystemName] = new
                    {
                        status = healthStatus.Status,
                        responseTimeMs = healthStatus.ResponseTimeMs,
                        lastCheckedAt = healthStatus.LastCheckedAt,
                        metrics = healthStatus.Metrics
                    };

                    if (healthStatus.Status == "UNHEALTHY")
                    {
                        unhealthyServices.Add(service.SystemName);
                        _logger.LogWarning("Service {Service} is unhealthy: {Error}",
                            service.SystemName, healthStatus.ErrorMessage);
                    }
                    else if (healthStatus.Status == "DEGRADED")
                    {
                        degradedServices.Add(service.SystemName);
                        _logger.LogWarning("Service {Service} is degraded", service.SystemName);
                    }

                    return healthStatus;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Health check failed for service: {Service}", service.SystemName);

                    serviceStatuses[service.SystemName] = new
                    {
                        status = "ERROR",
                        error = ex.Message
                    };

                    unhealthyServices.Add(service.SystemName);
                    return null;
                }
            });

            await Task.WhenAll(healthCheckTasks);

            // Determine overall health status
            if (unhealthyServices.Any())
            {
                var message = $"Unhealthy services: {string.Join(", ", unhealthyServices)}";
                _logger.LogError("External service health check failed: {Message}", message);

                return HealthCheckResult.Unhealthy(
                    description: message,
                    data: serviceStatuses);
            }

            if (degradedServices.Any())
            {
                var message = $"Degraded services: {string.Join(", ", degradedServices)}";
                _logger.LogWarning("External service health check degraded: {Message}", message);

                return HealthCheckResult.Degraded(
                    description: message,
                    data: serviceStatuses);
            }

            _logger.LogInformation("All external services are healthy");

            return HealthCheckResult.Healthy(
                description: "All external validation services are operational",
                data: serviceStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service health check encountered an error");

            return HealthCheckResult.Unhealthy(
                description: $"Health check error: {ex.Message}",
                exception: ex,
                data: serviceStatuses);
        }
    }
}
