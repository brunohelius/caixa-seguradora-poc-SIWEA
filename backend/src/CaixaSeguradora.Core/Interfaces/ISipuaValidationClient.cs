using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Interface for SIPUA (EFP Contract Validation) SOAP service client
/// Handles validation for claims with NUM_CONTRATO > 0
/// </summary>
public interface ISipuaValidationClient
{
    /// <summary>
    /// System name identifier
    /// </summary>
    string SystemName { get; }

    /// <summary>
    /// Validates payment authorization request against SIPUA SOAP service
    /// </summary>
    /// <param name="request">External validation request with contract details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation response with EZERT8 code and error details</returns>
    Task<ExternalValidationResponse> ValidateAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks SIPUA service health and availability
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is healthy and accessible</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a contract should be routed to SIPUA (EFP validation)
    /// </summary>
    /// <param name="numContrato">Contract number from EF_CONTR_SEG_HABIT lookup</param>
    /// <returns>True if contract exists and NUM_CONTRATO > 0</returns>
    bool SupportsContract(int? numContrato);
}
