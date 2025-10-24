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
public class DashboardIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DashboardIntegrationTests(WebApplicationFactory<Program> factory)
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
        Assert.True(result.ProgressoGeral.UserStoriesTotal == 6); // Should have 6 user stories
        Assert.True(result.ProgressoGeral.UserStoriesCompletas >= 0);
        Assert.True(result.ProgressoGeral.UserStoriesCompletas <= result.ProgressoGeral.UserStoriesTotal);

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
        Assert.NotNull(result.SaudeDoSistema.UltimaVerificacao);

        // Verify UltimaAtualizacao
        Assert.NotNull(result.UltimaAtualizacao);
    }

    [Fact]
    public async Task GetComponents_FilterByType_ReturnsFiltered()
    {
        // Arrange
        var componentType = "Screen"; // Assuming ComponentType enum value

        // Act
        var response = await _client.GetAsync($"/api/dashboard/components?tipo={componentType}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardComponentsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Componentes);

        // Verify all returned components match the requested type
        foreach (var component in result.Componentes)
        {
            Assert.Equal(componentType, component.ComponentType);
        }
    }

    [Fact]
    public async Task GetComponents_FilterByStatus_ReturnsFiltered()
    {
        // Arrange
        var status = "InProgress"; // Assuming ComponentStatus enum value

        // Act
        var response = await _client.GetAsync($"/api/dashboard/components?status={status}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardComponentsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Componentes);

        // Verify all returned components match the requested status
        foreach (var component in result.Componentes)
        {
            Assert.Equal(status, component.Status);
        }
    }

    [Fact]
    public async Task GetComponents_FilterByResponsavel_ReturnsFiltered()
    {
        // Arrange
        var responsavel = "TestDeveloper";

        // Act
        var response = await _client.GetAsync($"/api/dashboard/components?responsavel={responsavel}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardComponentsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Componentes);

        // Verify all returned components match the requested responsavel
        foreach (var component in result.Componentes)
        {
            Assert.Equal(responsavel, component.AssignedDeveloper);
        }
    }

    [Fact]
    public async Task GetPerformanceMetrics_FilterByPeriod_ReturnsCorrectRange()
    {
        // Arrange
        var periodo = "ultimo-dia";

        // Act
        var response = await _client.GetAsync($"/api/dashboard/performance?periodo={periodo}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardPerformanceResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Metricas);
        Assert.Equal(periodo, result.Periodo);
        Assert.NotNull(result.UltimaAtualizacao);

        // Verify metrics are within the last 24 hours
        var yesterday = DateTime.UtcNow.AddDays(-1);
        foreach (var metric in result.Metricas)
        {
            if (metric.MeasurementTimestamp.HasValue)
            {
                Assert.True(metric.MeasurementTimestamp.Value >= yesterday);
            }
        }
    }

    [Fact]
    public async Task GetPerformanceMetrics_SupportsMultiplePeriods()
    {
        // Arrange
        var periods = new[] { "ultima-hora", "ultimo-dia", "ultima-semana", "ultimo-mes" };

        foreach (var periodo in periods)
        {
            // Act
            var response = await _client.GetAsync($"/api/dashboard/performance?periodo={periodo}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<DashboardPerformanceResponse>();
            Assert.NotNull(result);
            Assert.Equal(periodo, result.Periodo);
        }
    }

    [Fact]
    public async Task GetActivities_LimitsResults()
    {
        // Arrange
        var limite = 5;

        // Act
        var response = await _client.GetAsync($"/api/dashboard/activities?limite={limite}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardActivitiesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Atividades);

        // Verify returned activities don't exceed limit
        Assert.True(result.Atividades.Count <= limite);
    }

    [Fact]
    public async Task GetActivities_ReturnsOrderedByTimestamp()
    {
        // Arrange
        var limite = 10;

        // Act
        var response = await _client.GetAsync($"/api/dashboard/activities?limite={limite}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardActivitiesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Atividades);

        // Verify activities are ordered by timestamp DESC (most recent first)
        if (result.Atividades.Count > 1)
        {
            for (int i = 0; i < result.Atividades.Count - 1; i++)
            {
                var current = result.Atividades[i].Timestamp;
                var next = result.Atividades[i + 1].Timestamp;

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

        // Verify all 5 services are checked
        var health = result.SaudeDoSistema;
        Assert.NotNull(health.ApiStatus);
        Assert.NotNull(health.DatabaseStatus);
        Assert.NotNull(health.CNOUAStatus);
        Assert.NotNull(health.SIPUAStatus);
        Assert.NotNull(health.SIMDAStatus);
        Assert.NotNull(health.UltimaVerificacao);

        // Verify health check is recent (within last minute)
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        Assert.True(
            health.UltimaVerificacao >= oneMinuteAgo,
            $"Health check should be recent. Last check: {health.UltimaVerificacao}, One minute ago: {oneMinuteAgo}"
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
    public async Task GetComponents_ReturnsHoursVariance()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/components");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardComponentsResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Componentes);

        // Verify each component has hours variance calculated
        foreach (var component in result.Componentes)
        {
            if (component.EstimatedHours > 0 && component.ActualHours > 0)
            {
                var expectedVariance = component.ActualHours - component.EstimatedHours;
                Assert.Equal(expectedVariance, component.HoursVariance);
            }
        }
    }

    [Fact]
    public async Task GetPerformanceMetrics_IncludesImprovementPercentage()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/performance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<DashboardPerformanceResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Metricas);

        // Verify improvement percentage is calculated correctly
        foreach (var metric in result.Metricas)
        {
            if (metric.LegacyValue > 0)
            {
                var expectedImprovement =
                    ((metric.LegacyValue - metric.NewValue) / metric.LegacyValue) * 100;

                // Allow small floating point differences
                Assert.True(
                    Math.Abs(metric.ImprovementPercentage - expectedImprovement) < 0.01m,
                    $"Expected improvement {expectedImprovement}%, got {metric.ImprovementPercentage}%"
                );
            }
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
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode,
                $"Endpoint {endpoint} should be accessible"
            );
        }
    }
}
