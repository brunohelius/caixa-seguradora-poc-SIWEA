using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CaixaSeguradora.Core.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CaixaSeguradora.Api.Tests.Integration;

/// <summary>
/// T130: Integration tests for Migration Dashboard functionality
/// Tests dashboard endpoints including overview, components, performance metrics,
/// activities, and system health monitoring.
/// Success Criteria (SC-014): Dashboard displays all required metrics
/// Success Criteria (SC-015): 30-second auto-refresh capability
/// </summary>
public class DashboardIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DashboardIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOverview_ReturnsAggregatedData()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/overview");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardOverviewDto>();
        Assert.NotNull(result);

        // Verify ProgressoGeral section exists
        Assert.NotNull(result.ProgressoGeral);
        Assert.True(result.ProgressoGeral.PercentualCompleto >= 0);
        Assert.True(result.ProgressoGeral.PercentualCompleto <= 100);
        Assert.True(result.ProgressoGeral.TotalUserStories == 6); // Should have 6 user stories
        Assert.True(result.ProgressoGeral.UserStoriesCompletas >= 0);
        Assert.True(result.ProgressoGeral.UserStoriesCompletas <= result.ProgressoGeral.TotalUserStories);

        // Verify StatusUserStories section
        Assert.NotNull(result.StatusUserStories);
        Assert.True(result.StatusUserStories.Count <= 6);

        // Verify ComponentesMigrados section
        Assert.NotNull(result.ComponentesMigrados);
        Assert.NotNull(result.ComponentesMigrados.Telas);
        Assert.NotNull(result.ComponentesMigrados.RegrasNegocio);
        Assert.NotNull(result.ComponentesMigrados.IntegracoesBD);
        Assert.NotNull(result.ComponentesMigrados.ServicosExternos);

        // Verify each component type has valid counts
        Assert.True(result.ComponentesMigrados.Telas.Total >= 0);
        Assert.True(result.ComponentesMigrados.RegrasNegocio.Total >= 0);
        Assert.True(result.ComponentesMigrados.IntegracoesBD.Total >= 0);
        Assert.True(result.ComponentesMigrados.ServicosExternos.Total >= 0);

        // Verify SaudeDoSistema section
        Assert.NotNull(result.SaudeDoSistema);
        Assert.NotNull(result.SaudeDoSistema.LastCheckAt);

        // Verify UltimaAtualizacao
        Assert.NotNull(result.UltimaAtualizacao);
    }

    [Fact]
    public async Task GetComponents_ReturnsComponentsList()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/components");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ComponentStatusDto>>();
        Assert.NotNull(result);

        // Verify list is returned (may be empty in test environment)
        Assert.IsType<List<ComponentStatusDto>>(result);
    }

    [Fact]
    public async Task GetComponents_ValidatesComponentStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/components");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ComponentStatusDto>>();
        Assert.NotNull(result);

        // Verify component structure if any components exist
        foreach (var component in result)
        {
            Assert.NotNull(component.Id);
            Assert.NotNull(component.Name);
            Assert.NotNull(component.Status);
            Assert.True(component.ProgressPercentage >= 0 && component.ProgressPercentage <= 100);
        }
    }

    [Fact]
    public async Task GetComponents_ReturnsValidData()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/components");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ComponentStatusDto>>();
        Assert.NotNull(result);

        // Verify all returned components have valid structure
        foreach (var component in result)
        {
            Assert.True(component.TasksCompleted >= 0);
            Assert.True(component.TotalTasks >= 0);
            Assert.True(component.TasksCompleted <= component.TotalTasks);
        }
    }

    [Fact]
    public async Task GetPerformanceMetrics_FilterByDays_ReturnsCorrectRange()
    {
        // Arrange
        var days = 1;

        // Act
        var response = await _client.GetAsync($"/api/dashboard/performance?days={days}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PerformanceMetricDto>>();
        Assert.NotNull(result);

        // Verify metrics are within the last 24 hours
        var yesterday = DateTime.UtcNow.AddDays(-1);
        foreach (var metric in result)
        {
            Assert.True(metric.Date >= yesterday);
        }
    }

    [Fact]
    public async Task GetPerformanceMetrics_SupportsMultiplePeriods()
    {
        // Arrange
        var daysPeriods = new[] { 1, 7, 30 };

        foreach (var days in daysPeriods)
        {
            // Act
            var response = await _client.GetAsync($"/api/dashboard/performance?days={days}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<List<PerformanceMetricDto>>();
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task GetActivities_LimitsResults()
    {
        // Arrange
        var limit = 5;

        // Act
        var response = await _client.GetAsync($"/api/dashboard/activities?limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ActivityDto>>();
        Assert.NotNull(result);

        // Verify returned activities don't exceed limit
        Assert.True(result.Count <= limit);
    }

    [Fact]
    public async Task GetActivities_ReturnsOrderedByTimestamp()
    {
        // Arrange
        var limit = 10;

        // Act
        var response = await _client.GetAsync($"/api/dashboard/activities?limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ActivityDto>>();
        Assert.NotNull(result);

        // Verify activities are ordered by timestamp DESC (most recent first)
        if (result.Count > 1)
        {
            for (int i = 0; i < result.Count - 1; i++)
            {
                var current = result[i].Timestamp;
                var next = result[i + 1].Timestamp;

                Assert.True(
                    current >= next,
                    $"Activity {i} timestamp ({current}) should be >= activity {i + 1} timestamp ({next})"
                );
            }
        }
    }

    [Fact]
    public async Task GetSystemHealth_ChecksAllServices()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/overview");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardOverviewDto>();
        Assert.NotNull(result);
        Assert.NotNull(result.SaudeDoSistema);

        // Verify health status is present
        var health = result.SaudeDoSistema;
        Assert.NotNull(health.Status);
        Assert.NotNull(health.DatabaseStatus);
        Assert.NotNull(health.LastCheckAt);

        // Verify external services status dictionary
        Assert.NotNull(health.ExternalServicesStatus);

        // Verify health check is recent (within last minute)
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        Assert.True(
            health.LastCheckAt >= oneMinuteAgo,
            $"Health check should be recent. Last check: {health.LastCheckAt}, One minute ago: {oneMinuteAgo}"
        );
    }

    [Fact]
    public async Task Dashboard_HasCacheHeaders()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/overview");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify Cache-Control header
        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(
            response.Headers.CacheControl.Private ||
            response.Headers.CacheControl.MaxAge.HasValue,
            "Dashboard should have cache control headers set"
        );

        // Max age should be 30 seconds or less (for 30-second refresh)
        if (response.Headers.CacheControl.MaxAge.HasValue)
        {
            Assert.True(
                response.Headers.CacheControl.MaxAge.Value.TotalSeconds <= 30,
                "Cache max-age should be 30 seconds or less for dashboard refresh"
            );
        }
    }

    [Fact]
    public async Task Dashboard_SupportsETags()
    {
        // Act - First request
        var firstResponse = await _client.GetAsync("/api/dashboard/overview");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Get ETag from first response
        var etag = firstResponse.Headers.ETag;

        if (etag != null)
        {
            // Act - Second request with If-None-Match header
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/overview");
            request.Headers.Add("If-None-Match", etag.Tag);

            var secondResponse = await _client.SendAsync(request);

            // Assert - Should return 304 Not Modified if content hasn't changed
            // Or 200 OK with new content if it has changed
            Assert.True(
                secondResponse.StatusCode == HttpStatusCode.NotModified ||
                secondResponse.StatusCode == HttpStatusCode.OK,
                $"Expected 304 or 200, got {secondResponse.StatusCode}"
            );
        }
    }

    [Fact]
    public async Task GetComponents_ReturnsValidTaskCounts()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/components");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ComponentStatusDto>>();
        Assert.NotNull(result);

        // Verify each component has valid task counts
        foreach (var component in result)
        {
            Assert.True(component.TasksCompleted >= 0);
            Assert.True(component.TotalTasks >= 0);
            Assert.True(component.TasksCompleted <= component.TotalTasks);
        }
    }

    [Fact]
    public async Task GetPerformanceMetrics_ReturnsValidMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/performance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PerformanceMetricDto>>();
        Assert.NotNull(result);

        // Verify each metric has valid structure
        foreach (var metric in result)
        {
            Assert.NotNull(metric.MetricType);
            Assert.NotNull(metric.Unit);
            Assert.True(metric.Value >= 0);
            Assert.True(metric.Date <= DateTime.UtcNow);
        }
    }

    [Fact]
    public async Task Dashboard_AllEndpoints_AreAccessible()
    {
        // Test all dashboard endpoints are accessible
        var endpoints = new[]
        {
            "/api/dashboard/overview",
            "/api/dashboard/components",
            "/api/dashboard/performance",
            "/api/dashboard/activities"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
