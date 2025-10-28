using Microsoft.Extensions.Logging;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// Routes external validation requests to appropriate service (CNOUA, SIPUA, SIMDA)
/// Based on product code and contract number
/// Implements routing logic per T022 specification:
/// - CNOUA: Product codes 6814, 7701, 7709 (consortium products)
/// - SIPUA: NUM_CONTRATO > 0 (EFP contracts)
/// - SIMDA: NUM_CONTRATO = 0 or null (HB contracts)
/// </summary>
public class ExternalServiceRouter
{
    private readonly ICnouaValidationClient _cnouaClient;
    private readonly ISipuaValidationClient _sipuaClient;
    private readonly ISimdaValidationClient _simdaClient;
    private readonly ILogger<ExternalServiceRouter> _logger;

    public ExternalServiceRouter(
        ICnouaValidationClient cnouaClient,
        ISipuaValidationClient sipuaClient,
        ISimdaValidationClient simdaClient,
        ILogger<ExternalServiceRouter> logger)
    {
        _cnouaClient = cnouaClient;
        _sipuaClient = sipuaClient;
        _simdaClient = simdaClient;
        _logger = logger;
    }

    /// <summary>
    /// Routes validation request to appropriate external service
    /// Priority: Product code routing (CNOUA) → Contract routing (SIPUA/SIMDA)
    /// </summary>
    /// <param name="request">External validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation response from routed service</returns>
    public async Task<ExternalValidationResponse> RouteAndValidateAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        // T022: Priority 1 - Check consortium product routing (CNOUA)
        if (_cnouaClient.SupportsProduct(request.CodProdu))
        {
            _logger.LogInformation(
                "Routing to CNOUA: Product={Product} is consortium type (6814/7701/7709)",
                request.CodProdu);

            return await _cnouaClient.ValidateAsync(request, cancellationToken);
        }

        // T022: Priority 2 - Check contract routing (SIPUA vs SIMDA)
        if (_sipuaClient.SupportsContract(request.NumContrato))
        {
            _logger.LogInformation(
                "Routing to SIPUA: Contract={Contract} is EFP type (NUM_CONTRATO > 0)",
                request.NumContrato);

            return await _sipuaClient.ValidateAsync(request, cancellationToken);
        }

        if (_simdaClient.SupportsContract(request.NumContrato))
        {
            _logger.LogInformation(
                "Routing to SIMDA: Contract={Contract} is HB type (NUM_CONTRATO = 0 or null)",
                request.NumContrato);

            return await _simdaClient.ValidateAsync(request, cancellationToken);
        }

        // No service supports this request - should never happen if routing logic is correct
        _logger.LogError(
            "No external service supports: Product={Product}, Contract={Contract}",
            request.CodProdu, request.NumContrato);

        return new ExternalValidationResponse
        {
            Ezert8 = "ROUTING_ERROR",
            ErrorMessage = "Nenhum serviço de validação disponível para este produto/contrato",
            ValidationService = "ROUTER",
            RequestTimestamp = DateTime.UtcNow,
            ResponseTimestamp = DateTime.UtcNow,
            ElapsedMilliseconds = 0
        };
    }

    /// <summary>
    /// Determines which service would handle the request without executing validation
    /// Useful for logging, metrics, and testing routing logic
    /// </summary>
    /// <param name="productCode">Product code from claim</param>
    /// <param name="contractNumber">Contract number from EF_CONTR_SEG_HABIT lookup</param>
    /// <returns>Service name (CNOUA, SIPUA, SIMDA, or NONE)</returns>
    public string DetermineService(int productCode, int? contractNumber)
    {
        if (_cnouaClient.SupportsProduct(productCode))
        {
            return "CNOUA";
        }

        if (_sipuaClient.SupportsContract(contractNumber))
        {
            return "SIPUA";
        }

        if (_simdaClient.SupportsContract(contractNumber))
        {
            return "SIMDA";
        }

        return "NONE";
    }

    /// <summary>
    /// Checks health of all external validation services
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with service name → health status</returns>
    public async Task<Dictionary<string, bool>> CheckAllServicesHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var healthChecks = new Dictionary<string, bool>();

        try
        {
            var cnouaHealth = await _cnouaClient.IsHealthyAsync(cancellationToken);
            healthChecks["CNOUA"] = cnouaHealth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CNOUA health check exception");
            healthChecks["CNOUA"] = false;
        }

        try
        {
            var sipuaHealth = await _sipuaClient.IsHealthyAsync(cancellationToken);
            healthChecks["SIPUA"] = sipuaHealth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SIPUA health check exception");
            healthChecks["SIPUA"] = false;
        }

        try
        {
            var simdaHealth = await _simdaClient.IsHealthyAsync(cancellationToken);
            healthChecks["SIMDA"] = simdaHealth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SIMDA health check exception");
            healthChecks["SIMDA"] = false;
        }

        _logger.LogInformation(
            "External services health: CNOUA={Cnoua}, SIPUA={Sipua}, SIMDA={Simda}",
            healthChecks["CNOUA"], healthChecks["SIPUA"], healthChecks["SIMDA"]);

        return healthChecks;
    }

    /// <summary>
    /// Gets routing statistics for monitoring and debugging
    /// </summary>
    /// <returns>Routing configuration summary</returns>
    public Dictionary<string, object> GetRoutingInfo()
    {
        return new Dictionary<string, object>
        {
            ["CNOUA_Products"] = new[] { 6814, 7701, 7709 },
            ["SIPUA_ContractRule"] = "NUM_CONTRATO > 0",
            ["SIMDA_ContractRule"] = "NUM_CONTRATO = 0 or null",
            ["RoutingPriority"] = new[] { "Product Code (CNOUA)", "Contract Type (SIPUA/SIMDA)" },
            ["AvailableServices"] = new[] { "CNOUA", "SIPUA", "SIMDA" }
        };
    }
}
