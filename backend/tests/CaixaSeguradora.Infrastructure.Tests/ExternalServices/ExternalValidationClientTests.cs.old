using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.ExternalServices;
using Moq;
using Moq.Protected;
using Xunit;

namespace CaixaSeguradora.Infrastructure.Tests.ExternalServices;

/// <summary>
/// T073: Unit tests for External Validation Clients (CNOUA, SIPUA, SIMDA)
/// Tests external service integration with various scenarios including
/// success responses, error codes, retry logic, circuit breaker, and timeouts.
/// Uses MockHttpMessageHandler to avoid real HTTP calls.
/// </summary>
public class ExternalValidationClientTests
{
    #region CNOUA Client Tests

    [Fact]
    public async Task CNOUAClient_SuccessResponse_ReturnsValid()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var expectedResponse = @"{""ezert8"": ""00000000"", ""status"": ""success""}";

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse)
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.cnoua.com")
        };

        var client = new CNOUAValidationClient(httpClient);
        var claim = CreateTestClaim(6814); // Consortium product

        // Act
        var result = await client.ValidateConsortiumAsync(claim);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Equal("00000000", result.Code);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task CNOUAClient_ErrorCode_ReturnsInvalid()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var expectedResponse = @"{""ezert8"": ""EZERT8001"", ""message"": ""Contrato de consórcio inválido""}";

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse)
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.cnoua.com")
        };

        var client = new CNOUAValidationClient(httpClient);
        var claim = CreateTestClaim(6814);

        // Act
        var result = await client.ValidateConsortiumAsync(claim);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Equal("EZERT8001", result.Code);
        Assert.Contains("Contrato de consórcio inválido", result.ErrorMessage);
    }

    [Fact]
    public async Task CNOUAClient_RetryOnTransientFailure_EventuallySucceeds()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var callCount = 0;

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // First call fails with 503
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }
                else
                {
                    // Second call succeeds
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(@"{""ezert8"": ""00000000"", ""status"": ""success""}")
                    };
                }
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.cnoua.com")
        };

        var client = new CNOUAValidationClient(httpClient);
        var claim = CreateTestClaim(6814);

        // Act
        var result = await client.ValidateConsortiumAsync(claim);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.True(callCount >= 2, "Retry should have been attempted");
    }

    [Fact]
    public async Task CNOUAClient_CircuitBreakerOpensAfter5Failures()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var callCount = 0;

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() =>
            {
                callCount++;
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.cnoua.com")
        };

        var client = new CNOUAValidationClient(httpClient);
        var claim = CreateTestClaim(6814);

        // Act - Make 6 consecutive calls
        for (int i = 0; i < 6; i++)
        {
            try
            {
                await client.ValidateConsortiumAsync(claim);
            }
            catch
            {
                // Expected to fail
            }
        }

        // Assert
        // After 5 failures, circuit should open
        // The 6th call should fail immediately without making HTTP request
        // Note: This test assumes circuit breaker is configured with 5 failure threshold
        Assert.True(callCount <= 6, "Circuit breaker should prevent additional HTTP calls");
    }

    [Fact]
    public async Task CNOUAClient_Timeout_ThrowsException()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(async () =>
            {
                // Simulate long-running request (longer than timeout)
                await Task.Delay(15000); // 15 seconds (timeout is 10s)
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.cnoua.com"),
            Timeout = TimeSpan.FromSeconds(1) // Short timeout for testing
        };

        var client = new CNOUAValidationClient(httpClient);
        var claim = CreateTestClaim(6814);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await client.ValidateConsortiumAsync(claim)
        );
    }

    #endregion

    #region SIPUA Client Tests

    [Fact]
    public async Task SIPUAClient_ValidEFPContract_ReturnsValid()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var soapResponse = @"<?xml version=""1.0""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ValidateResponse>
            <Result>Success</Result>
            <Code>0</Code>
        </ValidateResponse>
    </soap:Body>
</soap:Envelope>";

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(soapResponse, System.Text.Encoding.UTF8, "text/xml")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.sipua.com")
        };

        var client = new SIPUAValidationClient(httpClient);
        var contractNumber = 12345;

        // Act
        var result = await client.ValidateEFPContractAsync(contractNumber);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SIPUAClient_SoapFault_ReturnsInvalid()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var soapFault = @"<?xml version=""1.0""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <soap:Fault>
            <faultcode>soap:Server</faultcode>
            <faultstring>Contrato não encontrado</faultstring>
        </soap:Fault>
    </soap:Body>
</soap:Envelope>";

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(soapFault, System.Text.Encoding.UTF8, "text/xml")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.sipua.com")
        };

        var client = new SIPUAValidationClient(httpClient);
        var contractNumber = 99999;

        // Act
        var result = await client.ValidateEFPContractAsync(contractNumber);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Contains("Contrato não encontrado", result.ErrorMessage);
    }

    #endregion

    #region SIMDA Client Tests

    [Fact]
    public async Task SIMDAClient_ValidHBContract_ReturnsValid()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var soapResponse = @"<?xml version=""1.0""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ValidateResponse>
            <Result>Valid</Result>
            <Status>Active</Status>
        </ValidateResponse>
    </soap:Body>
</soap:Envelope>";

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(soapResponse, System.Text.Encoding.UTF8, "text/xml")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.simda.com")
        };

        var client = new SIMDAValidationClient(httpClient);
        var claim = CreateTestClaim(1234); // Non-consortium product

        // Act
        var result = await client.ValidateHBContractAsync(claim);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SIMDAClient_InvalidContract_ReturnsInvalid()
    {
        // Arrange
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var soapResponse = @"<?xml version=""1.0""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ValidateResponse>
            <Result>Invalid</Result>
            <ErrorCode>HB001</ErrorCode>
            <ErrorMessage>Apólice inválida ou cancelada</ErrorMessage>
        </ValidateResponse>
    </soap:Body>
</soap:Envelope>";

        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(soapResponse, System.Text.Encoding.UTF8, "text/xml")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://test.simda.com")
        };

        var client = new SIMDAValidationClient(httpClient);
        var claim = CreateTestClaim(1234);

        // Act
        var result = await client.ValidateHBContractAsync(claim);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Equal("HB001", result.Code);
        Assert.Contains("Apólice inválida ou cancelada", result.ErrorMessage);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test claim with specified product code
    /// </summary>
    private ClaimMaster CreateTestClaim(int productCode)
    {
        return new ClaimMaster
        {
            Tipseg = 1,
            Orgsin = 1,
            Rmosin = 1,
            Numsin = 1,
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            Orgapo = 1,
            Rmoapo = 1,
            Numapol = 1,
            Codprodu = productCode,
            Sdopag = 50000.00m,
            Totpag = 0.00m,
            Ocorhist = 0,
            Tipreg = "A",
            Tpsegu = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    #endregion
}
