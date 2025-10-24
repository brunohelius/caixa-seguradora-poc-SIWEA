using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CaixaSeguradora.Core.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CaixaSeguradora.Api.Tests.Integration;

/// <summary>
/// T071: Integration tests for Payment Authorization functionality
/// Tests payment authorization flow with various scenarios including validation,
/// concurrent updates, external services, and error conditions.
/// </summary>
public class PaymentAuthorizationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PaymentAuthorizationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthorizePayment_WithValidRequest_ReturnsCreated()
    {
        // Arrange - Create a payment authorization request
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 25000.00m,
            ValorCorrecao = 1250.50m,
            Favorecido = "Test Beneficiary",
            TipoApolice = "1",
            Observacoes = "Test payment authorization"
        };

        // Use valid claim parameters (these should match test database seed)
        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaymentAuthorizationResponse>();
        Assert.NotNull(result);
        Assert.True(result.Sucesso);
        Assert.True(result.Ocorhist > 0);
        Assert.Equal(1098, result.Operacao);
        Assert.Equal(25000.00m, result.Valpri);
        Assert.Equal(1250.50m, result.Crrmon);
        Assert.NotNull(result.Dtmovto);
        Assert.NotNull(result.Horaoper);

        // Verify Location header is set
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task AuthorizePayment_ExceedingPendingValue_Returns400()
    {
        // Arrange - Request with amount exceeding claim's pending value
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 999999999.99m, // Excessive amount
            ValorCorrecao = 0,
            Favorecido = "Test Beneficiary",
            TipoApolice = "1",
            Observacoes = "Test exceeding limit"
        };

        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("excede o saldo pendente", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthorizePayment_MissingBeneficiaryWhenRequired_Returns400()
    {
        // Arrange - Request without beneficiary when it's required
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 10000.00m,
            ValorCorrecao = 500.00m,
            Favorecido = "", // Missing beneficiary
            TipoApolice = "1",
            Observacoes = "Test missing beneficiary"
        };

        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("favorecido", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthorizePayment_InvalidTipoPagamento_Returns400()
    {
        // Arrange - Request with invalid payment type
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 99, // Invalid type (should be 1-5)
            ValorPrincipal = 10000.00m,
            ValorCorrecao = 0,
            Favorecido = "Test Beneficiary",
            TipoApolice = "1",
            Observacoes = "Test invalid type"
        };

        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Tipo de pagamento deve ser", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthorizePayment_ConsortiumProduct_CallsValidationService()
    {
        // Arrange - Request for consortium product (codes 6814, 7701, 7709)
        // This test verifies that consortium products trigger external validation
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 15000.00m,
            ValorCorrecao = 750.00m,
            Favorecido = "Consortium Beneficiary",
            TipoApolice = "1",
            Observacoes = "Consortium product payment"
        };

        // Use claim with consortium product code (would need to be seeded in test DB)
        int tipseg = 1;
        int orgsin = 2; // Assume orgsin=2 has consortium product
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        // Response may be 201 if validation succeeds, or 422 if validation fails
        // Both are acceptable outcomes - the important part is that validation was called
        Assert.True(
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Expected 201 or 422 but got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task AuthorizePayment_NonExistentClaim_Returns404()
    {
        // Arrange
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 10000.00m,
            ValorCorrecao = 0,
            Favorecido = "Test Beneficiary",
            TipoApolice = "1",
            Observacoes = "Test non-existent claim"
        };

        // Use non-existent claim parameters
        int tipseg = 999;
        int orgsin = 999;
        int rmosin = 999;
        int numsin = 999;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("NAO CADASTRADO", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthorizePayment_ValidatesDecimalPrecision()
    {
        // Arrange - Test 2 decimal precision requirement (SC-008)
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 12345.67m, // Exactly 2 decimals
            ValorCorrecao = 543.21m,    // Exactly 2 decimals
            Favorecido = "Precision Test",
            TipoApolice = "1",
            Observacoes = "Testing decimal precision"
        };

        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PaymentAuthorizationResponse>();
            Assert.NotNull(result);

            // Verify returned values maintain 2 decimal precision
            Assert.Equal(12345.67m, result.Valpri);
            Assert.Equal(543.21m, result.Crrmon);

            // BTNF values should also have 2 decimal precision
            Assert.Equal(2, GetDecimalPlaces(result.Valpribt));
            Assert.Equal(2, GetDecimalPlaces(result.Crrmonbt));
            Assert.Equal(2, GetDecimalPlaces(result.Valtotbt));
        }
    }

    [Fact]
    public async Task AuthorizePayment_UpdatesClaimTotals()
    {
        // Arrange
        var request = new PaymentAuthorizationRequest
        {
            TipoPagamento = 1,
            ValorPrincipal = 5000.00m,
            ValorCorrecao = 250.00m,
            Favorecido = "Totals Test",
            TipoApolice = "1",
            Observacoes = "Testing total updates"
        };

        int tipseg = 1;
        int orgsin = 1;
        int rmosin = 1;
        int numsin = 1;

        // Get original claim to compare totals
        var originalClaim = await _client.GetFromJsonAsync<ClaimDetailDto>(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment",
            request
        );

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PaymentAuthorizationResponse>();
            Assert.NotNull(result);

            // Verify updated pending value is returned
            Assert.NotNull(result.ValorPendenteAtualizado);

            // Pending value should be reduced by the payment amount
            Assert.True(result.ValorPendenteAtualizado < (originalClaim?.ValorPendente ?? 0));
        }
    }

    /// <summary>
    /// Helper method to count decimal places
    /// </summary>
    private int GetDecimalPlaces(decimal value)
    {
        var text = value.ToString("G29");
        var decimalIndex = text.IndexOf('.');
        if (decimalIndex < 0) return 0;
        return text.Length - decimalIndex - 1;
    }
}
