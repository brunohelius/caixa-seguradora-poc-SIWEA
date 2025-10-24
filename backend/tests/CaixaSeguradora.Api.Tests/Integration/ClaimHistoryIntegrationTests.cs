using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CaixaSeguradora.Core.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CaixaSeguradora.Api.Tests.Integration;

/// <summary>
/// T082: Integration tests for Claim History functionality
/// Tests retrieving payment history with pagination, ordering, and formatting.
/// </summary>
public class ClaimHistoryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ClaimHistoryIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHistory_WithMultipleRecords_ReturnsPaginated()
    {
        // Arrange - Assumes test database has claim with multiple history records
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        int page = 1;
        int pageSize = 20;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page={page}&pageSize={pageSize}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);
        Assert.True(result.Sucesso);
        Assert.NotNull(result.Historico);

        // Verify pagination metadata
        Assert.Equal(page, result.PaginaAtual);
        Assert.Equal(pageSize, result.TamanhoPagina);
        Assert.True(result.TotalRegistros >= 0);
        Assert.True(result.TotalPaginas >= 0);

        // If there are records, verify they don't exceed page size
        if (result.Historico.Count > 0)
        {
            Assert.True(result.Historico.Count <= pageSize);
        }
    }

    [Fact]
    public async Task GetHistory_OrderedByOcorrenciaMostRecentFirst()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=1&pageSize=100"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Historico);

        // Verify records are ordered by Ocorhist in descending order (most recent first)
        if (result.Historico.Count > 1)
        {
            for (int i = 0; i < result.Historico.Count - 1; i++)
            {
                Assert.True(
                    result.Historico[i].Ocorhist >= result.Historico[i + 1].Ocorhist,
                    $"Record {i} (ocorhist={result.Historico[i].Ocorhist}) should be >= record {i + 1} (ocorhist={result.Historico[i + 1].Ocorhist})"
                );
            }
        }
    }

    [Fact]
    public async Task GetHistory_FormatsDateCorrectly()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=1&pageSize=10"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Historico);

        // Verify date format for each record
        foreach (var record in result.Historico)
        {
            // DataHoraFormatada should match "dd/MM/yyyy HH:mm:ss" pattern
            Assert.NotNull(record.DataHoraFormatada);
            Assert.Matches(
                @"^\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}$",
                record.DataHoraFormatada
            );

            // Verify dtmovto is present
            Assert.NotNull(record.Dtmovto);

            // Verify horaoper is present
            Assert.NotNull(record.Horaoper);
        }
    }

    [Fact]
    public async Task GetHistory_ForNonExistentClaim_Returns404()
    {
        // Arrange - Use non-existent claim parameters
        int tipseg = 999;
        int orgsin = 999;
        int rmosin = 999;
        int numsin = 999;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history"
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("NAO CADASTRADO", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetHistory_WithCustomPageSize_RespectsLimit()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;
        int customPageSize = 5;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=1&pageSize={customPageSize}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Historico);

        // Verify returned records don't exceed custom page size
        Assert.True(result.Historico.Count <= customPageSize);
        Assert.Equal(customPageSize, result.TamanhoPagina);
    }

    [Fact]
    public async Task GetHistory_WithMaxPageSize_DoesNotExceed100()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;
        int requestedPageSize = 200; // Exceeds max of 100

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=1&pageSize={requestedPageSize}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Historico);

        // Verify page size was capped at 100
        Assert.True(result.TamanhoPagina <= 100);
        Assert.True(result.Historico.Count <= 100);
    }

    [Fact]
    public async Task GetHistory_SecondPage_ReturnsCorrectRecords()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;
        int pageSize = 10;

        // Get first page
        var page1Response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=1&pageSize={pageSize}"
        );
        var page1Result = await page1Response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();

        // Skip if not enough records for second page
        if (page1Result?.TotalRegistros <= pageSize)
        {
            return; // Not enough data to test pagination
        }

        // Act - Get second page
        var page2Response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=2&pageSize={pageSize}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

        var page2Result = await page2Response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(page2Result);
        Assert.NotNull(page2Result.Historico);

        Assert.Equal(2, page2Result.PaginaAtual);
        Assert.True(page2Result.Historico.Count > 0);

        // Verify page 2 records are different from page 1
        if (page1Result.Historico.Count > 0 && page2Result.Historico.Count > 0)
        {
            var firstRecordPage1 = page1Result.Historico[0].Ocorhist;
            var firstRecordPage2 = page2Result.Historico[0].Ocorhist;

            Assert.NotEqual(firstRecordPage1, firstRecordPage2);
        }
    }

    [Fact]
    public async Task GetHistory_ContainsAllRequiredFields()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history?page=1&pageSize=10"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Historico);

        // Verify each record contains all required fields
        foreach (var record in result.Historico)
        {
            Assert.True(record.Tipseg > 0);
            Assert.True(record.Orgsin > 0);
            Assert.True(record.Rmosin > 0);
            Assert.True(record.Numsin > 0);
            Assert.True(record.Ocorhist >= 0);
            Assert.True(record.Operacao > 0);
            Assert.NotNull(record.Dtmovto);
            Assert.NotNull(record.Horaoper);
            Assert.NotNull(record.DataHoraFormatada);
            Assert.True(record.Valpri >= 0);
            Assert.True(record.Valpribt >= 0);
            Assert.True(record.Valtotbt >= 0);
            Assert.NotNull(record.Sitcontb);
            Assert.NotNull(record.Situacao);
        }
    }

    [Fact]
    public async Task GetHistory_NoCacheHeader_AlwaysFetchesLatest()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify Cache-Control header is set to no-cache
        if (response.Headers.CacheControl != null)
        {
            Assert.True(
                response.Headers.CacheControl.NoCache ||
                response.Headers.CacheControl.NoStore,
                "History should not be cached (Cache-Control: no-cache or no-store expected)"
            );
        }
    }

    [Fact]
    public async Task GetHistory_DefaultPagination_UsesCorrectDefaults()
    {
        // Arrange
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act - Call without page parameters
        var response = await _client.GetAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimHistoryResponse>();
        Assert.NotNull(result);

        // Verify defaults: page = 1, pageSize = 20
        Assert.Equal(1, result.PaginaAtual);
        Assert.Equal(20, result.TamanhoPagina);
    }
}

/// <summary>
/// Response DTO for claim history endpoint
/// </summary>
public class ClaimHistoryResponse
{
    public bool Sucesso { get; set; }
    public int TotalRegistros { get; set; }
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas { get; set; }
    public List<HistoryRecordDto> Historico { get; set; } = new();
}
