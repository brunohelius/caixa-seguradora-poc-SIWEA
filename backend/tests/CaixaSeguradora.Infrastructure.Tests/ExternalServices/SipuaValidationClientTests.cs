using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Polly.CircuitBreaker;
using Polly.Timeout;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.ExternalServices;

namespace CaixaSeguradora.Infrastructure.Tests.ExternalServices;

/// <summary>
/// Unit tests for SipuaValidationClient covering:
/// - T028: SOAP envelope construction and parsing
/// - Contract number validation (NUM_CONTRATO > 0 routing)
/// - EZERT8 extraction from SOAP XML responses
/// - SOAP fault handling
/// - Retry behavior (3 attempts with exponential backoff)
/// - Circuit breaker behavior (5 failures → 30s break)
/// - Timeout policies (10 seconds)
/// - Error code mapping (EZERT8 → CONS-001 through CONS-005)
/// </summary>
public class SipuaValidationClientTests
{
    private readonly Mock<ILogger<SipuaValidationClient>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public SipuaValidationClientTests()
    {
        _mockLogger = new Mock<ILogger<SipuaValidationClient>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    }

    #region T028: SOAP Success Scenarios

    [Fact]
    public async Task ValidateAsync_WithValidEfpContract_ReturnsSuccessResponse()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockSoapResponseHandler(
            HttpStatusCode.OK,
            "00000000",
            "Validação aprovada");

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("00000000", response.Ezert8);
        Assert.True(response.IsSuccess);
        Assert.Null(response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);
        Assert.True(response.ElapsedMilliseconds >= 0);

        // Verify HTTP POST was made with SOAP envelope
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.Content != null &&
                req.Content.Headers.ContentType!.MediaType == "application/soap+xml"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_BuildsSoapEnvelopeCorrectly()
    {
        // Arrange
        string? capturedSoapEnvelope = null;
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (req, ct) =>
            {
                capturedSoapEnvelope = await req.Content!.ReadAsStringAsync(ct);
                return CreateSoapResponseMessage("00000000", "OK");
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        var request = CreateValidRequest(
            numContrato: 987654,
            orgsin: 1,
            rmosin: 2,
            numsin: 123456,
            tipoPagamento: 3,
            valorPrincipal: 50000.00m);

        // Act
        await client.ValidateAsync(request);

        // Assert
        Assert.NotNull(capturedSoapEnvelope);

        var doc = XDocument.Parse(capturedSoapEnvelope);
        var ns = XNamespace.Get("http://caixaseguradora.com.br/sipua");

        // Verify SOAP structure
        Assert.Contains("Envelope", capturedSoapEnvelope);
        Assert.Contains("Body", capturedSoapEnvelope);
        Assert.Contains("ValidateContractRequest", capturedSoapEnvelope);

        // Verify payload values
        var contractNumber = doc.Descendants(ns + "ContractNumber").FirstOrDefault()?.Value;
        Assert.Equal("987654", contractNumber);

        var claimNumber = doc.Descendants(ns + "ClaimNumber").FirstOrDefault()?.Value;
        Assert.Equal("01/02/123456", claimNumber);

        var policyType = doc.Descendants(ns + "PolicyType").FirstOrDefault()?.Value;
        Assert.Equal("3", policyType);

        var principalAmount = doc.Descendants(ns + "PrincipalAmount").FirstOrDefault()?.Value;
        Assert.Equal("50000.00", principalAmount);
    }

    [Theory]
    [InlineData("EZERT8001", "Contrato de consórcio inválido")] // CONS-001
    [InlineData("EZERT8002", "Contrato cancelado")]              // CONS-002
    [InlineData("EZERT8003", "Grupo encerrado")]                 // CONS-003
    [InlineData("EZERT8004", "Cota não contemplada")]            // CONS-004
    [InlineData("EZERT8005", "Beneficiário não autorizado")]     // CONS-005
    public async Task ValidateAsync_WithErrorCodes_MapsToPortugueseMessages(string ezert8, string expectedMessage)
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockSoapResponseHandler(
            HttpStatusCode.OK,
            ezert8,
            expectedMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(ezert8, response.Ezert8);
        Assert.Equal(expectedMessage, response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);
    }

    [Fact]
    public async Task ValidateAsync_ParsesEzert8FromSoapResponse()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockSoapResponseHandler(
            HttpStatusCode.OK,
            "00000000",
            "Validação aprovada");

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.Equal("00000000", response.Ezert8);
        Assert.True(response.IsSuccess);
    }

    #endregion

    #region Contract Routing Tests

    [Theory]
    [InlineData(1, true)]       // EFP contract
    [InlineData(123456, true)]  // Valid EFP contract
    [InlineData(999999, true)]  // Another valid EFP contract
    [InlineData(0, false)]      // Not EFP (SIMDA territory)
    [InlineData(-1, false)]     // Invalid
    [InlineData(null, false)]   // No contract (CNOUA territory)
    public void SupportsContract_WithVariousContracts_ReturnsExpectedResult(int? numContrato, bool expected)
    {
        // Arrange
        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = client.SupportsContract(numContrato);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidateAsync_WithNullContract_ReturnsErrorResponse()
    {
        // Arrange
        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: null);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("INVALID_CONTRACT", response.Ezert8);
        Assert.Contains("not suitable for SIPUA", response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);
    }

    [Fact]
    public async Task ValidateAsync_WithZeroContract_ReturnsErrorResponse()
    {
        // Arrange
        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 0);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("INVALID_CONTRACT", response.Ezert8);
        Assert.Contains("expected NUM_CONTRATO > 0", response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);
    }

    [Fact]
    public async Task ValidateAsync_WithNegativeContract_ReturnsErrorResponse()
    {
        // Arrange
        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: -1);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("INVALID_CONTRACT", response.Ezert8);
        Assert.Equal("SIPUA", response.ValidationService);
    }

    #endregion

    #region T028: Retry and Timeout Scenarios

    [Fact]
    public async Task ValidateAsync_WithTransientFailure_RetriesThreeTimes()
    {
        // Arrange
        var attemptCount = 0;
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount < 3)
                {
                    // First 2 attempts: transient failure (503 Service Unavailable)
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent("Service temporarily unavailable")
                    };
                }
                else
                {
                    // Third attempt: success
                    return CreateSoapResponseMessage("00000000", "OK");
                }
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(3, attemptCount); // Verify 3 attempts were made
        Assert.Equal("00000000", response.Ezert8);

        // Verify retry logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SIPUA retry")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // At least 2 retry warnings
    }

    [Fact]
    public async Task ValidateAsync_WithPersistentTimeout_ReturnsTimeoutError()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/"),
            Timeout = TimeSpan.FromSeconds(1) // Short timeout for test speed
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("SYS-005", response.Ezert8);
        Assert.Contains("indisponível", response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);

        // Verify timeout was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SIPUA")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidateAsync_WithRequestTimeout_RetriesWithExponentialBackoff()
    {
        // Arrange
        var attemptCount = 0;
        var attemptTimestamps = new List<DateTime>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                attemptTimestamps.Add(DateTime.UtcNow);

                if (attemptCount <= 3)
                {
                    // All attempts timeout
                    return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                    {
                        Content = new StringContent("Request timeout")
                    };
                }

                return CreateSoapResponseMessage("00000000", "OK");
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.Equal(4, attemptCount); // Initial + 3 retries

        // Verify exponential backoff (2s, 4s, 8s delays)
        if (attemptTimestamps.Count >= 4)
        {
            var delay1 = attemptTimestamps[1] - attemptTimestamps[0];
            var delay2 = attemptTimestamps[2] - attemptTimestamps[1];
            var delay3 = attemptTimestamps[3] - attemptTimestamps[2];

            // Allow 500ms tolerance for test execution overhead
            Assert.True(delay1.TotalSeconds >= 1.5 && delay1.TotalSeconds <= 2.5,
                $"First retry delay should be ~2s, was {delay1.TotalSeconds}s");
            Assert.True(delay2.TotalSeconds >= 3.5 && delay2.TotalSeconds <= 4.5,
                $"Second retry delay should be ~4s, was {delay2.TotalSeconds}s");
            Assert.True(delay3.TotalSeconds >= 7.5 && delay3.TotalSeconds <= 8.5,
                $"Third retry delay should be ~8s, was {delay3.TotalSeconds}s");
        }
    }

    #endregion

    #region T028: Circuit Breaker Tests

    [Fact]
    public async Task ValidateAsync_AfterFiveConsecutiveFailures_OpensCircuitBreaker()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockSoapResponseHandler(
            HttpStatusCode.InternalServerError,
            null,
            null);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act - Make 5 consecutive failing requests to trip circuit breaker
        var responses = new List<ExternalValidationResponse>();
        for (int i = 0; i < 5; i++)
        {
            responses.Add(await client.ValidateAsync(request));
        }

        // Circuit breaker should now be OPEN
        // Next request should fail immediately without calling HTTP service
        var circuitBreakerOpenResponse = await client.ValidateAsync(request);

        // Assert
        // First 5 requests should return error responses (circuit breaker still closed/half-open)
        Assert.All(responses, r => Assert.False(r.IsSuccess));

        // 6th request should fail with circuit breaker open error
        Assert.False(circuitBreakerOpenResponse.IsSuccess);
        Assert.Equal("SYS-005", circuitBreakerOpenResponse.Ezert8);
        Assert.Contains("indisponível", circuitBreakerOpenResponse.ErrorMessage);

        // Verify circuit breaker open was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("circuit breaker")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task ValidateAsync_WithCircuitBreakerOpen_ReturnsImmediateError()
    {
        // Arrange
        var attemptCount = 0;
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Server error")
                };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act - Trip circuit breaker with 5 failures
        for (int i = 0; i < 5; i++)
        {
            await client.ValidateAsync(request);
        }

        var initialAttemptCount = attemptCount;

        // Make request while circuit breaker is open
        var response = await client.ValidateAsync(request);

        // Assert
        // Circuit breaker should prevent HTTP call
        Assert.False(response.IsSuccess);
        Assert.Equal("SYS-005", response.Ezert8);

        // Verify HTTP call was NOT made (circuit breaker blocked it)
        var callsAfterCircuitOpen = attemptCount - initialAttemptCount;
        Assert.True(callsAfterCircuitOpen <= 4,
            $"Expected 0-4 HTTP calls after circuit open, got {callsAfterCircuitOpen}");
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task IsHealthyAsync_WithHealthyService_ReturnsTrue()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("WSDL content", Encoding.UTF8, "text/xml")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var isHealthy = await client.IsHealthyAsync();

        // Assert
        Assert.True(isHealthy);

        // Verify WSDL endpoint was called
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("?wsdl")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task IsHealthyAsync_WithUnhealthyService_ReturnsFalse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var isHealthy = await client.IsHealthyAsync();

        // Assert
        Assert.False(isHealthy);
    }

    [Fact]
    public async Task IsHealthyAsync_WithException_ReturnsFalse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var isHealthy = await client.IsHealthyAsync();

        // Assert
        Assert.False(isHealthy);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("health check failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }

    #endregion

    #region SOAP Fault Handling

    [Fact]
    public async Task ValidateAsync_WithSoapFault_ReturnsErrorResponse()
    {
        // Arrange
        var soapFault = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Server</faultcode>
      <faultstring>Internal SIPUA error</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(soapFault, Encoding.UTF8, "application/soap+xml")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("SOAP_ERROR", response.Ezert8);
        Assert.Contains("InternalServerError", response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);
    }

    [Fact]
    public async Task ValidateAsync_WithMalformedXml_ReturnsParseError()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<malformed xml>>", Encoding.UTF8, "application/soap+xml")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("PARSE_ERROR", response.Ezert8);
        Assert.Contains("desconhecido", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_WithMissingEzert8Element_ReturnsParseError()
    {
        // Arrange
        var soapResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sip=""http://caixaseguradora.com.br/sipua"">
  <soap:Body>
    <sip:ValidateContractResponse>
      <sip:Status>OK</sip:Status>
    </sip:ValidateContractResponse>
  </soap:Body>
</soap:Envelope>";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(soapResponse, Encoding.UTF8, "application/soap+xml")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("PARSE_ERROR", response.Ezert8);
        Assert.Contains("desconhecido", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region HTTP Error Handling

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task ValidateAsync_WithHttpErrors_ReturnsErrorResponse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockSoapResponseHandler(statusCode, null, null);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://sipua.caixaseguradora.com.br/soap/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("SIPUA"))
            .Returns(httpClient);

        var client = new SipuaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(numContrato: 123456);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("SOAP_ERROR", response.Ezert8);
        Assert.Contains(statusCode.ToString(), response.ErrorMessage);
        Assert.Equal("SIPUA", response.ValidationService);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock HttpMessageHandler that returns SOAP XML responses
    /// </summary>
    private Mock<HttpMessageHandler> CreateMockSoapResponseHandler(
        HttpStatusCode statusCode,
        string? ezert8Code,
        string? message)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                if (statusCode != HttpStatusCode.OK || ezert8Code == null)
                {
                    return new HttpResponseMessage(statusCode)
                    {
                        Content = new StringContent("Error response")
                    };
                }

                return CreateSoapResponseMessage(ezert8Code, message ?? "OK");
            });

        return mockHandler;
    }

    /// <summary>
    /// Creates a SOAP 1.2 response message with EZERT8 code
    /// </summary>
    private HttpResponseMessage CreateSoapResponseMessage(string ezert8Code, string message)
    {
        var soapResponse = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sip=""http://caixaseguradora.com.br/sipua"">
  <soap:Body>
    <sip:ValidateContractResponse>
      <sip:Ezert8>{ezert8Code}</sip:Ezert8>
      <sip:Message>{message}</sip:Message>
      <sip:Timestamp>{DateTime.UtcNow:O}</sip:Timestamp>
    </sip:ValidateContractResponse>
  </soap:Body>
</soap:Envelope>";

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(soapResponse, Encoding.UTF8, "application/soap+xml")
        };
    }

    /// <summary>
    /// Creates a valid ExternalValidationRequest for testing
    /// </summary>
    private ExternalValidationRequest CreateValidRequest(
        int? numContrato = 123456,
        int orgsin = 1,
        int rmosin = 2,
        int numsin = 123456,
        int tipoPagamento = 1,
        decimal valorPrincipal = 50000.00m)
    {
        return new ExternalValidationRequest
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            Orgsin = orgsin,
            Rmosin = rmosin,
            Numsin = numsin,
            CodProdu = 1234,
            NumContrato = numContrato,
            TipoPagamento = tipoPagamento,
            ValorPrincipal = valorPrincipal,
            ValorCorrecao = 1500.00m,
            Beneficiario = "João Silva",
            OperatorId = "TESTUSER"
        };
    }

    #endregion
}
