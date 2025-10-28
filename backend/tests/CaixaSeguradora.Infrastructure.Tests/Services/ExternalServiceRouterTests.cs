using Microsoft.Extensions.Logging;
using Moq;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Services;

namespace CaixaSeguradora.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for ExternalServiceRouter covering:
/// - T030: Routing logic (product code → CNOUA, contract → SIPUA/SIMDA)
/// - Priority rules (product takes precedence over contract)
/// - RouteAndValidateAsync execution
/// - DetermineService method
/// - Health check aggregation
/// - Routing metadata (GetRoutingInfo)
/// </summary>
public class ExternalServiceRouterTests
{
    private readonly Mock<ICnouaValidationClient> _mockCnouaClient;
    private readonly Mock<ISipuaValidationClient> _mockSipuaClient;
    private readonly Mock<ISimdaValidationClient> _mockSimdaClient;
    private readonly Mock<ILogger<ExternalServiceRouter>> _mockLogger;
    private readonly ExternalServiceRouter _router;

    public ExternalServiceRouterTests()
    {
        _mockCnouaClient = new Mock<ICnouaValidationClient>();
        _mockSipuaClient = new Mock<ISipuaValidationClient>();
        _mockSimdaClient = new Mock<ISimdaValidationClient>();
        _mockLogger = new Mock<ILogger<ExternalServiceRouter>>();

        _router = new ExternalServiceRouter(
            _mockCnouaClient.Object,
            _mockSipuaClient.Object,
            _mockSimdaClient.Object,
            _mockLogger.Object);

        // Setup default SupportsProduct/SupportsContract behavior
        SetupDefaultClientSupport();
    }

    #region T030: Routing Logic Tests - CNOUA (Product-based)

    [Theory]
    [InlineData(6814)] // Consortium product 1
    [InlineData(7701)] // Consortium product 2
    [InlineData(7709)] // Consortium product 3
    public async Task RouteAndValidateAsync_WithConsortiumProduct_RoutesToCnoua(int productCode)
    {
        // Arrange
        var request = CreateValidationRequest(codProdu: productCode, numContrato: 123456);
        var expectedResponse = CreateSuccessResponse("CNOUA");

        _mockCnouaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.Equal("CNOUA", response.ValidationService);
        Assert.True(response.IsSuccess);

        // Verify CNOUA was called, others were not
        _mockCnouaClient.Verify(c => c.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockSipuaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSimdaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Routing to CNOUA")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(6814, "CNOUA")]
    [InlineData(7701, "CNOUA")]
    [InlineData(7709, "CNOUA")]
    public void DetermineService_WithConsortiumProducts_ReturnsCnoua(int productCode, string expectedService)
    {
        // Act
        var service = _router.DetermineService(productCode, null);

        // Assert
        Assert.Equal(expectedService, service);
    }

    #endregion

    #region T030: Routing Logic Tests - SIPUA (Contract-based)

    [Fact]
    public async Task RouteAndValidateAsync_WithEfpContract_RoutesToSipua()
    {
        // Arrange
        var request = CreateValidationRequest(codProdu: 1234, numContrato: 123456); // Non-consortium, EFP contract
        var expectedResponse = CreateSuccessResponse("SIPUA");

        _mockSipuaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.Equal("SIPUA", response.ValidationService);
        Assert.True(response.IsSuccess);

        // Verify SIPUA was called, others were not
        _mockCnouaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSipuaClient.Verify(c => c.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockSimdaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Routing to SIPUA")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1234, 1, "SIPUA")]
    [InlineData(1234, 123456, "SIPUA")]
    [InlineData(9999, 999999, "SIPUA")]
    public void DetermineService_WithEfpContracts_ReturnsSipua(int productCode, int contractNumber, string expectedService)
    {
        // Act
        var service = _router.DetermineService(productCode, contractNumber);

        // Assert
        Assert.Equal(expectedService, service);
    }

    #endregion

    #region T030: Routing Logic Tests - SIMDA (HB Contract-based)

    [Fact]
    public async Task RouteAndValidateAsync_WithHbContract_RoutesToSimda()
    {
        // Arrange
        var request = CreateValidationRequest(codProdu: 1234, numContrato: 0); // Non-consortium, HB contract
        var expectedResponse = CreateSuccessResponse("SIMDA");

        _mockSimdaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.Equal("SIMDA", response.ValidationService);
        Assert.True(response.IsSuccess);

        // Verify SIMDA was called, others were not
        _mockCnouaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSipuaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSimdaClient.Verify(c => c.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Routing to SIMDA")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RouteAndValidateAsync_WithNullContract_RoutesToSimda()
    {
        // Arrange
        var request = CreateValidationRequest(codProdu: 1234, numContrato: null); // Non-consortium, no contract
        var expectedResponse = CreateSuccessResponse("SIMDA");

        _mockSimdaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.Equal("SIMDA", response.ValidationService);
        Assert.True(response.IsSuccess);
    }

    [Theory]
    [InlineData(1234, 0, "SIMDA")]
    [InlineData(1234, null, "SIMDA")]
    [InlineData(9999, 0, "SIMDA")]
    public void DetermineService_WithHbContracts_ReturnsSimda(int productCode, int? contractNumber, string expectedService)
    {
        // Act
        var service = _router.DetermineService(productCode, contractNumber);

        // Assert
        Assert.Equal(expectedService, service);
    }

    #endregion

    #region T030: Priority Rules Tests

    [Fact]
    public async Task RouteAndValidateAsync_WithConsortiumProductAndContract_PrioritizesProduct()
    {
        // Arrange: Request has consortium product (6814) AND EFP contract (123456)
        // Expected: Product routing takes precedence → CNOUA
        var request = CreateValidationRequest(codProdu: 6814, numContrato: 123456);
        var expectedResponse = CreateSuccessResponse("CNOUA");

        _mockCnouaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.Equal("CNOUA", response.ValidationService);

        // Verify CNOUA was called (product priority), SIPUA was not
        _mockCnouaClient.Verify(c => c.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockSipuaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSimdaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void DetermineService_WithConsortiumProductAndContract_ReturnsCnoua()
    {
        // Arrange: Product 7701 (consortium) with EFP contract 123456
        // Expected: Product priority → CNOUA
        var service = _router.DetermineService(7701, 123456);

        // Assert
        Assert.Equal("CNOUA", service);
    }

    #endregion

    #region T030: Error Handling Tests

    [Fact]
    public async Task RouteAndValidateAsync_WithUnsupportedRequest_ReturnsRoutingError()
    {
        // Arrange: Request that no service supports (should never happen in practice)
        var request = CreateValidationRequest(codProdu: 9999, numContrato: -1);

        // Override client support to reject this request
        _mockCnouaClient.Setup(c => c.SupportsProduct(9999)).Returns(false);
        _mockSipuaClient.Setup(c => c.SupportsContract(-1)).Returns(false);
        _mockSimdaClient.Setup(c => c.SupportsContract(-1)).Returns(false);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("ROUTING_ERROR", response.Ezert8);
        Assert.Equal("ROUTER", response.ValidationService);
        Assert.Contains("Nenhum serviço de validação disponível", response.ErrorMessage);

        // Verify no service was called
        _mockCnouaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSipuaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSimdaClient.Verify(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verify error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No external service supports")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DetermineService_WithUnsupportedRequest_ReturnsNone()
    {
        // Arrange: Override client support
        _mockCnouaClient.Setup(c => c.SupportsProduct(9999)).Returns(false);
        _mockSipuaClient.Setup(c => c.SupportsContract(-1)).Returns(false);
        _mockSimdaClient.Setup(c => c.SupportsContract(-1)).Returns(false);

        // Act
        var service = _router.DetermineService(9999, -1);

        // Assert
        Assert.Equal("NONE", service);
    }

    #endregion

    #region T030: Health Check Aggregation Tests

    [Fact]
    public async Task CheckAllServicesHealthAsync_WithAllHealthy_ReturnsAllTrue()
    {
        // Arrange
        _mockCnouaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockSipuaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockSimdaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var healthStatus = await _router.CheckAllServicesHealthAsync();

        // Assert
        Assert.Equal(3, healthStatus.Count);
        Assert.True(healthStatus["CNOUA"]);
        Assert.True(healthStatus["SIPUA"]);
        Assert.True(healthStatus["SIMDA"]);

        // Verify all services were checked
        _mockCnouaClient.Verify(c => c.IsHealthyAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockSipuaClient.Verify(c => c.IsHealthyAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockSimdaClient.Verify(c => c.IsHealthyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAllServicesHealthAsync_WithSomeUnhealthy_ReturnsPartialHealth()
    {
        // Arrange
        _mockCnouaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockSipuaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false); // Unhealthy
        _mockSimdaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var healthStatus = await _router.CheckAllServicesHealthAsync();

        // Assert
        Assert.Equal(3, healthStatus.Count);
        Assert.True(healthStatus["CNOUA"]);
        Assert.False(healthStatus["SIPUA"]); // Should be false
        Assert.True(healthStatus["SIMDA"]);
    }

    [Fact]
    public async Task CheckAllServicesHealthAsync_WithHealthCheckException_ReturnsFalse()
    {
        // Arrange
        _mockCnouaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockSipuaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Connection timeout"));
        _mockSimdaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var healthStatus = await _router.CheckAllServicesHealthAsync();

        // Assert
        Assert.Equal(3, healthStatus.Count);
        Assert.True(healthStatus["CNOUA"]);
        Assert.False(healthStatus["SIPUA"]); // Exception should return false
        Assert.True(healthStatus["SIMDA"]);

        // Verify exception was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SIPUA health check exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckAllServicesHealthAsync_WithAllExceptions_ReturnsAllFalse()
    {
        // Arrange
        _mockCnouaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("CNOUA error"));
        _mockSipuaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SIPUA error"));
        _mockSimdaClient.Setup(c => c.IsHealthyAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SIMDA error"));

        // Act
        var healthStatus = await _router.CheckAllServicesHealthAsync();

        // Assert
        Assert.Equal(3, healthStatus.Count);
        Assert.False(healthStatus["CNOUA"]);
        Assert.False(healthStatus["SIPUA"]);
        Assert.False(healthStatus["SIMDA"]);

        // Verify all exceptions were logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
    }

    #endregion

    #region T030: Routing Metadata Tests

    [Fact]
    public void GetRoutingInfo_ReturnsCompleteRoutingConfiguration()
    {
        // Act
        var routingInfo = _router.GetRoutingInfo();

        // Assert
        Assert.NotNull(routingInfo);
        Assert.Equal(5, routingInfo.Count);

        // Verify CNOUA products
        Assert.Contains("CNOUA_Products", routingInfo.Keys);
        var cnouaProducts = routingInfo["CNOUA_Products"] as int[];
        Assert.NotNull(cnouaProducts);
        Assert.Equal(3, cnouaProducts.Length);
        Assert.Contains(6814, cnouaProducts);
        Assert.Contains(7701, cnouaProducts);
        Assert.Contains(7709, cnouaProducts);

        // Verify SIPUA contract rule
        Assert.Contains("SIPUA_ContractRule", routingInfo.Keys);
        Assert.Equal("NUM_CONTRATO > 0", routingInfo["SIPUA_ContractRule"]);

        // Verify SIMDA contract rule
        Assert.Contains("SIMDA_ContractRule", routingInfo.Keys);
        Assert.Equal("NUM_CONTRATO = 0 or null", routingInfo["SIMDA_ContractRule"]);

        // Verify routing priority
        Assert.Contains("RoutingPriority", routingInfo.Keys);
        var priority = routingInfo["RoutingPriority"] as string[];
        Assert.NotNull(priority);
        Assert.Equal(2, priority.Length);
        Assert.Equal("Product Code (CNOUA)", priority[0]);
        Assert.Equal("Contract Type (SIPUA/SIMDA)", priority[1]);

        // Verify available services
        Assert.Contains("AvailableServices", routingInfo.Keys);
        var services = routingInfo["AvailableServices"] as string[];
        Assert.NotNull(services);
        Assert.Equal(3, services.Length);
        Assert.Contains("CNOUA", services);
        Assert.Contains("SIPUA", services);
        Assert.Contains("SIMDA", services);
    }

    #endregion

    #region T030: Validation Response Propagation Tests

    [Fact]
    public async Task RouteAndValidateAsync_PropagatesValidationErrors()
    {
        // Arrange: CNOUA returns validation error
        var request = CreateValidationRequest(codProdu: 6814, numContrato: 123456);
        var errorResponse = new ExternalValidationResponse
        {
            Ezert8 = "EZERT8001",
            ErrorMessage = "Contrato de consórcio inválido",
            ValidationService = "CNOUA",
            RequestTimestamp = DateTime.UtcNow,
            ResponseTimestamp = DateTime.UtcNow,
            ElapsedMilliseconds = 150
        };

        _mockCnouaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("EZERT8001", response.Ezert8);
        Assert.Equal("Contrato de consórcio inválido", response.ErrorMessage);
        Assert.Equal("CNOUA", response.ValidationService);
    }

    [Fact]
    public async Task RouteAndValidateAsync_PropagatesElapsedTime()
    {
        // Arrange
        var request = CreateValidationRequest(codProdu: 1234, numContrato: 123456);
        var expectedResponse = new ExternalValidationResponse
        {
            Ezert8 = "00000000",
            ErrorMessage = null,
            ValidationService = "SIPUA",
            RequestTimestamp = DateTime.UtcNow.AddSeconds(-2),
            ResponseTimestamp = DateTime.UtcNow,
            ElapsedMilliseconds = 2500
        };

        _mockSipuaClient
            .Setup(c => c.ValidateAsync(It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _router.RouteAndValidateAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(2500, response.ElapsedMilliseconds);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up default client support behavior based on routing rules
    /// </summary>
    private void SetupDefaultClientSupport()
    {
        // CNOUA: Supports consortium products (6814, 7701, 7709)
        _mockCnouaClient.Setup(c => c.SupportsProduct(6814)).Returns(true);
        _mockCnouaClient.Setup(c => c.SupportsProduct(7701)).Returns(true);
        _mockCnouaClient.Setup(c => c.SupportsProduct(7709)).Returns(true);
        _mockCnouaClient.Setup(c => c.SupportsProduct(It.Is<int>(p => p != 6814 && p != 7701 && p != 7709))).Returns(false);

        // SIPUA: Supports EFP contracts (NUM_CONTRATO > 0)
        _mockSipuaClient.Setup(c => c.SupportsContract(It.Is<int?>(n => n.HasValue && n.Value > 0))).Returns(true);
        _mockSipuaClient.Setup(c => c.SupportsContract(It.Is<int?>(n => !n.HasValue || n.Value <= 0))).Returns(false);

        // SIMDA: Supports HB contracts (NUM_CONTRATO = 0 or null)
        _mockSimdaClient.Setup(c => c.SupportsContract(It.Is<int?>(n => !n.HasValue || n.Value == 0))).Returns(true);
        _mockSimdaClient.Setup(c => c.SupportsContract(It.Is<int?>(n => n.HasValue && n.Value != 0))).Returns(false);
    }

    /// <summary>
    /// Creates a valid ExternalValidationRequest for testing
    /// </summary>
    private ExternalValidationRequest CreateValidationRequest(int codProdu, int? numContrato)
    {
        return new ExternalValidationRequest
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            Orgsin = 1,
            Rmosin = 2,
            Numsin = 123456,
            CodProdu = codProdu,
            NumContrato = numContrato,
            TipoPagamento = 1,
            ValorPrincipal = 50000.00m,
            ValorCorrecao = 1500.00m,
            Beneficiario = "João Silva",
            OperatorId = "TESTUSER"
        };
    }

    /// <summary>
    /// Creates a success validation response
    /// </summary>
    private ExternalValidationResponse CreateSuccessResponse(string serviceName)
    {
        return new ExternalValidationResponse
        {
            Ezert8 = "00000000",
            ErrorMessage = null,
            ValidationService = serviceName,
            RequestTimestamp = DateTime.UtcNow,
            ResponseTimestamp = DateTime.UtcNow,
            ElapsedMilliseconds = 100
        };
    }

    #endregion
}
