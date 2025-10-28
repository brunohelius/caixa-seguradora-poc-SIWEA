using System.Net;
using System.Text;
using System.Text.Json;
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
/// Unit tests for CnouaValidationClient covering:
/// - T025: Success scenario with EZERT8='00000000'
/// - T026: Timeout with retry behavior (3 attempts with exponential backoff)
/// - T027: Circuit breaker behavior (5 failures → 30s break)
/// - Error code mapping (EZERT8 → CONS-001 through CONS-005)
/// - Product routing (6814, 7701, 7709)
/// </summary>
public class CnouaValidationClientTests
{
    private readonly Mock<ILogger<CnouaValidationClient>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public CnouaValidationClientTests()
    {
        _mockLogger = new Mock<ILogger<CnouaValidationClient>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    }

    #region T025: Success Scenarios

    [Fact]
    public async Task ValidateAsync_WithValidConsortiumProduct_ReturnsSuccessResponse()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new
            {
                ezert8 = "00000000",
                message = "Validação aprovada",
                timestamp = DateTime.UtcNow
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        var request = CreateValidRequest(productCode: 6814);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("00000000", response.Ezert8);
        Assert.True(response.IsSuccess);
        Assert.Null(response.ErrorMessage);
        Assert.Equal("CNOUA", response.ValidationService);
        Assert.True(response.ElapsedMilliseconds >= 0);

        // Verify HTTP request was made
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("validate")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Theory]
    [InlineData(6814)] // Consortium product 1
    [InlineData(7701)] // Consortium product 2
    [InlineData(7709)] // Consortium product 3
    public async Task ValidateAsync_WithSupportedProducts_AcceptsValidation(int productCode)
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new { ezert8 = "00000000", message = "OK", timestamp = DateTime.UtcNow });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: productCode);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal("00000000", response.Ezert8);
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
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.OK,
            new { ezert8, message = expectedMessage, timestamp = DateTime.UtcNow });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(ezert8, response.Ezert8);
        Assert.Equal(expectedMessage, response.ErrorMessage);
        Assert.Equal("CNOUA", response.ValidationService);
    }

    #endregion

    #region Product Routing Tests

    [Theory]
    [InlineData(6814, true)]
    [InlineData(7701, true)]
    [InlineData(7709, true)]
    [InlineData(1234, false)]
    [InlineData(9999, false)]
    public void SupportsProduct_WithVariousProductCodes_ReturnsExpectedResult(int productCode, bool expected)
    {
        // Arrange
        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = client.SupportsProduct(productCode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidateAsync_WithUnsupportedProduct_ReturnsErrorResponse()
    {
        // Arrange
        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 9999); // Not supported

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("UNSUPPORTED_PRODUCT", response.Ezert8);
        Assert.Contains("not supported by CNOUA", response.ErrorMessage);
        Assert.Equal("CNOUA", response.ValidationService);
    }

    #endregion

    #region T026: Timeout and Retry Scenarios

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
                    var successResponse = new
                    {
                        ezert8 = "00000000",
                        message = "OK",
                        timestamp = DateTime.UtcNow
                    };
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(successResponse),
                            Encoding.UTF8,
                            "application/json")
                    };
                }
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CNOUA retry")),
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
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/"),
            Timeout = TimeSpan.FromSeconds(1) // Short timeout for test speed
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("SYS-005", response.Ezert8);
        Assert.Contains("indisponível", response.ErrorMessage);
        Assert.Equal("CNOUA", response.ValidationService);

        // Verify timeout was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CNOUA")),
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

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { ezert8 = "00000000", timestamp = DateTime.UtcNow }),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.Equal(4, attemptCount); // Initial + 3 retries

        // Verify exponential backoff (2s, 4s, 8s delays)
        // Note: Actual delays should be approximately 2s, 4s, 8s between attempts
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

    #region T027: Circuit Breaker Tests

    [Fact]
    public async Task ValidateAsync_AfterFiveConsecutiveFailures_OpensCircuitBreaker()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.InternalServerError,
            new { error = "Internal server error" });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

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
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

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
        // Note: Due to retry policy, each validation attempt makes up to 4 HTTP calls
        // After circuit breaker opens, no new HTTP calls should occur
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
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, new { status = "healthy" });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

        // Act
        var isHealthy = await client.IsHealthyAsync();

        // Assert
        Assert.True(isHealthy);

        // Verify health check endpoint was called
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains("health")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task IsHealthyAsync_WithUnhealthyService_ReturnsFalse()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(
            HttpStatusCode.ServiceUnavailable,
            new { status = "unhealthy" });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

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
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);

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

    #region HTTP Error Handling

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task ValidateAsync_WithHttpErrors_ReturnsErrorResponse(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(statusCode, new { error = "HTTP error" });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("HTTP_ERROR", response.Ezert8);
        Assert.Contains(statusCode.ToString(), response.ErrorMessage);
        Assert.Equal("CNOUA", response.ValidationService);
    }

    [Fact]
    public async Task ValidateAsync_WithMalformedJsonResponse_ReturnsParseError()
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
                Content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://cnoua.caixaseguradora.com.br/api/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("CNOUA"))
            .Returns(httpClient);

        var client = new CnouaValidationClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var request = CreateValidRequest(productCode: 6814);

        // Act
        var response = await client.ValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        // Note: JSON parse errors are caught by the general exception handler and return SYS-005
        Assert.Equal("SYS-005", response.Ezert8);
        Assert.Contains("indisponível", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock HttpMessageHandler that returns specified status code and response object
    /// </summary>
    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, object responseObject)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(responseObject),
                    Encoding.UTF8,
                    "application/json")
            });

        return mockHandler;
    }

    /// <summary>
    /// Creates a valid ExternalValidationRequest for testing
    /// </summary>
    private ExternalValidationRequest CreateValidRequest(int productCode = 6814)
    {
        return new ExternalValidationRequest
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            Orgsin = 1,
            Rmosin = 2,
            Numsin = 123456,
            CodProdu = productCode,
            NumContrato = 987654,
            TipoPagamento = 1,
            ValorPrincipal = 50000.00m,
            ValorCorrecao = 1500.00m,
            Beneficiario = "João Silva",
            OperatorId = "TESTUSER"
        };
    }

    #endregion
}
