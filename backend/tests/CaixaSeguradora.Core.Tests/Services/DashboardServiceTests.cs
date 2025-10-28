using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CaixaSeguradora.Core.Tests.Services;

/// <summary>
/// T156-T175 [US8] - Dashboard Service Unit Tests
/// Tests dashboard metrics, user story progress, and system health monitoring
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<IClaimRepository> _mockClaimRepository;
    private readonly Mock<HealthCheckService> _mockHealthCheckService;
    private readonly Mock<ILogger<DashboardService>> _mockLogger;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockClaimRepository = new Mock<IClaimRepository>();
        _mockHealthCheckService = new Mock<HealthCheckService>();
        _mockLogger = new Mock<ILogger<DashboardService>>();

        _service = new DashboardService(
            _mockClaimRepository.Object,
            _mockHealthCheckService.Object,
            _mockLogger.Object);
    }

    #region GetOverviewAsync Tests

    [Fact]
    public async Task GetOverviewAsync_ReturnsCompleteDashboard()
    {
        // Arrange
        SetupHealthySystem();
        SetupClaimRepository(50);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ProgressPercentage >= 0 && result.ProgressPercentage <= 100);
        Assert.True(result.TotalTasks > 0);
        Assert.True(result.TotalUserStories > 0);
        Assert.NotNull(result.ProgressoGeral);
        Assert.NotNull(result.StatusUserStories);
        Assert.NotNull(result.ComponentesMigrados);
        Assert.NotNull(result.SaudeDoSistema);
    }

    [Fact]
    public async Task GetOverviewAsync_CalculatesProgressPercentageCorrectly()
    {
        // Arrange
        SetupHealthySystem();
        SetupClaimRepository(100);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        var expectedPercentage = (decimal)result.CompletedTasks / result.TotalTasks * 100;
        Assert.Equal(Math.Round(expectedPercentage, 2), result.ProgressPercentage);
        Assert.Equal(result.ProgressPercentage, result.ProgressoGeral.PercentualCompleto);
    }

    [Fact]
    public async Task GetOverviewAsync_CountsTasksCorrectly()
    {
        // Arrange
        SetupHealthySystem();
        SetupClaimRepository(75);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.Equal(result.CompletedTasks + result.InProgressTasks + result.PendingTasks, result.TotalTasks);
        Assert.True(result.CompletedTasks >= 0);
        Assert.True(result.InProgressTasks >= 0);
        Assert.True(result.PendingTasks >= 0);
    }

    [Fact]
    public async Task GetOverviewAsync_CountsUserStoriesCorrectly()
    {
        // Arrange
        SetupHealthySystem();
        SetupClaimRepository(25);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.True(result.CompletedUserStories <= result.TotalUserStories);
        Assert.True(result.CompletedUserStories >= 0);
        Assert.Equal(result.ProgressoGeral.TotalUserStories, result.TotalUserStories);
        Assert.Equal(result.ProgressoGeral.UserStoriesCompletas, result.CompletedUserStories);
    }

    [Fact]
    public async Task GetOverviewAsync_IncludesClaimsMigrated()
    {
        // Arrange
        var expectedClaimsCount = 150;
        SetupHealthySystem();
        SetupClaimRepository(expectedClaimsCount);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.Equal(expectedClaimsCount, result.TotalClaimsMigrated);
    }

    [Fact]
    public async Task GetOverviewAsync_IncludesSystemHealth()
    {
        // Arrange
        SetupHealthySystem();
        SetupClaimRepository(50);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.NotNull(result.SaudeDoSistema);
        Assert.Equal("HEALTHY", result.SaudeDoSistema.Status);
    }

    [Fact]
    public async Task GetOverviewAsync_WithDegradedHealth_ReflectsStatus()
    {
        // Arrange
        SetupDegradedSystem();
        SetupClaimRepository(50);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.NotNull(result.SaudeDoSistema);
        Assert.Equal("DEGRADED", result.SaudeDoSistema.Status);
    }

    [Fact]
    public async Task GetOverviewAsync_IncludesUptimeSeconds()
    {
        // Arrange
        SetupHealthySystem();
        SetupClaimRepository(50);

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.True(result.UptimeSeconds >= 0);
    }

    #endregion

    #region GetComponentsAsync Tests

    [Fact]
    public async Task GetComponentsAsync_ReturnsUserStories()
    {
        // Act
        var result = await _service.GetComponentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, component =>
        {
            Assert.NotNull(component.Id);
            Assert.NotNull(component.Name);
            Assert.NotNull(component.Status);
            Assert.True(component.ProgressPercentage >= 0 && component.ProgressPercentage <= 100);
        });
    }

    [Fact]
    public async Task GetComponentsAsync_ReturnsAtLeast5UserStories()
    {
        // Act
        var result = await _service.GetComponentsAsync();

        // Assert
        Assert.True(result.Count() >= 5);
    }

    [Fact]
    public async Task GetComponentsAsync_ContainsMixedStatuses()
    {
        // Act
        var result = await _service.GetComponentsAsync();

        // Assert
        var statuses = result.Select(c => c.Status).Distinct().ToList();
        Assert.Contains("COMPLETED", statuses);
        // May contain IN_PROGRESS and PENDING as well
    }

    #endregion

    #region GetPerformanceMetricsAsync Tests

    [Fact]
    public async Task GetPerformanceMetricsAsync_ReturnsMetrics()
    {
        // Act
        var result = await _service.GetPerformanceMetricsAsync(30);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPerformanceMetricsAsync_ReturnsCorrectNumberOfDays()
    {
        // Arrange
        var days = 15;

        // Act
        var result = await _service.GetPerformanceMetricsAsync(days);

        // Assert
        var uniqueDates = result.Select(m => m.Date.Date).Distinct().Count();
        Assert.Equal(days, uniqueDates);
    }

    [Fact]
    public async Task GetPerformanceMetricsAsync_IncludesMultipleMetricTypes()
    {
        // Act
        var result = await _service.GetPerformanceMetricsAsync(7);

        // Assert
        var metricTypes = result.Select(m => m.MetricType).Distinct().ToList();
        Assert.Contains("API_RESPONSE_TIME", metricTypes);
        Assert.Contains("THROUGHPUT", metricTypes);
        Assert.Contains("ERROR_RATE", metricTypes);
    }

    [Fact]
    public async Task GetPerformanceMetricsAsync_MetricsHaveValidValues()
    {
        // Act
        var result = await _service.GetPerformanceMetricsAsync(10);

        // Assert
        Assert.All(result, metric =>
        {
            Assert.True(metric.Value >= 0);
            Assert.NotNull(metric.Unit);
            Assert.NotNull(metric.MetricType);
        });
    }

    [Fact]
    public async Task GetPerformanceMetricsAsync_WithCustomDays_ReturnsCorrectRange()
    {
        // Arrange
        var days = 45;

        // Act
        var result = await _service.GetPerformanceMetricsAsync(days);

        // Assert
        var dates = result.Select(m => m.Date).OrderBy(d => d).ToList();
        var oldestDate = dates.First();
        var newestDate = dates.Last();
        var daysDifference = (newestDate - oldestDate).Days;
        Assert.True(daysDifference >= days - 1); // Allow for same-day metrics
    }

    #endregion

    #region GetRecentActivitiesAsync Tests

    [Fact]
    public async Task GetRecentActivitiesAsync_ReturnsActivities()
    {
        // Act
        var result = await _service.GetRecentActivitiesAsync(50);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_RespectsLimit()
    {
        // Arrange
        var limit = 3;

        // Act
        var result = await _service.GetRecentActivitiesAsync(limit);

        // Assert
        Assert.True(result.Count() <= limit);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ActivitiesHaveRequiredFields()
    {
        // Act
        var result = await _service.GetRecentActivitiesAsync(10);

        // Assert
        Assert.All(result, activity =>
        {
            Assert.NotNull(activity.Id);
            Assert.NotNull(activity.Type);
            Assert.NotNull(activity.Title);
            Assert.NotNull(activity.Description);
            Assert.True(activity.Timestamp <= DateTime.UtcNow);
            Assert.NotNull(activity.Severity);
        });
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_OrderedByTimestampDescending()
    {
        // Act
        var result = await _service.GetRecentActivitiesAsync(20);

        // Assert
        var activities = result.ToList();
        for (int i = 0; i < activities.Count - 1; i++)
        {
            Assert.True(activities[i].Timestamp >= activities[i + 1].Timestamp);
        }
    }

    #endregion

    #region GetSystemHealthAsync Tests

    [Fact]
    public async Task GetSystemHealthAsync_WithHealthySystem_ReturnsHealthy()
    {
        // Arrange
        SetupHealthySystem();

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HEALTHY", result.Status);
        Assert.NotNull(result.DatabaseStatus);
        Assert.True(result.DatabaseResponseTimeMs >= 0);
    }

    [Fact]
    public async Task GetSystemHealthAsync_WithDegradedSystem_ReturnsDegraded()
    {
        // Arrange
        SetupDegradedSystem();

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        Assert.Equal("DEGRADED", result.Status);
    }

    [Fact]
    public async Task GetSystemHealthAsync_WithUnhealthySystem_ReturnsUnhealthy()
    {
        // Arrange
        SetupUnhealthySystem();

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        Assert.Equal("UNHEALTHY", result.Status);
    }

    [Fact]
    public async Task GetSystemHealthAsync_IncludesPerformanceMetrics()
    {
        // Arrange
        SetupHealthySystem();

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        Assert.True(result.MemoryUsagePercentage >= 0);
        Assert.True(result.CpuUsagePercentage >= 0);
        Assert.True(result.ActiveRequestsCount >= 0);
        Assert.True(result.RequestsPerMinute >= 0);
        Assert.True(result.AverageResponseTimeMs >= 0);
    }

    [Fact]
    public async Task GetSystemHealthAsync_WithException_ReturnsUnhealthy()
    {
        // Arrange
        _mockHealthCheckService.Setup(h => h.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Health check failed"));

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        Assert.Equal("UNHEALTHY", result.Status);
        Assert.Equal("UNKNOWN", result.DatabaseStatus);
    }

    [Fact]
    public async Task GetSystemHealthAsync_IncludesHealthCheckDetails()
    {
        // Arrange
        SetupHealthySystem();

        // Act
        var result = await _service.GetSystemHealthAsync();

        // Assert
        Assert.NotNull(result.HealthChecks);
        // May have health check details depending on setup
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task GetOverviewAsync_WithNoClaimsData_HandlesGracefully()
    {
        // Arrange
        SetupHealthySystem();
        _mockClaimRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimMaster>());

        // Act
        var result = await _service.GetOverviewAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalClaimsMigrated);
        Assert.Equal(0, result.ClaimsProcessedToday);
    }

    [Fact]
    public async Task GetOverviewAsync_WithClaimRepositoryException_ThrowsException()
    {
        // Arrange
        SetupHealthySystem();
        _mockClaimRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetOverviewAsync());
    }

    [Fact]
    public async Task GetPerformanceMetricsAsync_WithZeroDays_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetPerformanceMetricsAsync(0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_WithZeroLimit_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetRecentActivitiesAsync(0);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Helper Methods

    private void SetupHealthySystem()
    {
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(
                    HealthStatus.Healthy,
                    "Database is healthy",
                    TimeSpan.FromMilliseconds(45),
                    null,
                    null)
            },
            TimeSpan.FromMilliseconds(50));

        _mockHealthCheckService.Setup(h => h.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);
    }

    private void SetupDegradedSystem()
    {
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(
                    HealthStatus.Degraded,
                    "Database response time is high",
                    TimeSpan.FromMilliseconds(250),
                    null,
                    null)
            },
            TimeSpan.FromMilliseconds(300));

        _mockHealthCheckService.Setup(h => h.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);
    }

    private void SetupUnhealthySystem()
    {
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(
                    HealthStatus.Unhealthy,
                    "Database connection failed",
                    TimeSpan.FromMilliseconds(5000),
                    new InvalidOperationException("Connection timeout"),
                    null)
            },
            TimeSpan.FromMilliseconds(5100));

        _mockHealthCheckService.Setup(h => h.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);
    }

    private void SetupClaimRepository(int claimCount)
    {
        var claims = new List<ClaimMaster>();
        var today = DateTime.UtcNow.Date;

        for (int i = 0; i < claimCount; i++)
        {
            claims.Add(new ClaimMaster
            {
                Tipseg = 1,
                Orgsin = 100,
                Rmosin = 200,
                Numsin = 1000 + i,
                Fonte = 1,
                Protsini = 100000 + i,
                Dac = i % 10,
                Sdopag = 10000.00m,
                Totpag = 5000.00m * (i % 3),
                CreatedAt = i % 5 == 0 ? today : today.AddDays(-i % 30) // 20% created today
            });
        }

        _mockClaimRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);
    }

    #endregion
}
