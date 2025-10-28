using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Interface for CNOUA (Consortium) validation service client
/// Handles validation for consortium product codes: 6814, 7701, 7709
/// </summary>
public interface ICnouaValidationClient
{
    /// <summary>
    /// System name identifier
    /// </summary>
    string SystemName { get; }

    /// <summary>
    /// Validates payment authorization request against CNOUA service
    /// </summary>
    /// <param name="request">External validation request with claim and product details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation response with EZERT8 code and error details</returns>
    Task<ExternalValidationResponse> ValidateAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks CNOUA service health and availability
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is healthy and accessible</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a product code should be routed to CNOUA
    /// </summary>
    /// <param name="productCode">Product code to check</param>
    /// <returns>True if product should use CNOUA validation (6814, 7701, 7709)</returns>
    bool SupportsProduct(int productCode);
}
