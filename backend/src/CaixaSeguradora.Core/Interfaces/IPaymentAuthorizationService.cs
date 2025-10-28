using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Service for payment authorization operations
/// </summary>
public interface IPaymentAuthorizationService
{
    /// <summary>
    /// Authorizes a payment request with external validation
    /// </summary>
    /// <param name="request">Payment authorization request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment authorization response</returns>
    Task<PaymentAuthorizationResponse> AuthorizePaymentAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets authorization status by authorization ID
    /// </summary>
    /// <param name="authorizationId">Authorization identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authorization response or null if not found</returns>
    Task<PaymentAuthorizationResponse?> GetAuthorizationStatusAsync(
        string authorizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all authorizations for a specific claim
    /// </summary>
    /// <param name="claimId">Claim identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of authorization responses</returns>
    Task<IEnumerable<PaymentAuthorizationResponse>> GetClaimAuthorizationsAsync(
        int claimId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending authorization
    /// </summary>
    /// <param name="authorizationId">Authorization identifier</param>
    /// <param name="cancelledBy">User cancelling the authorization</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if cancelled successfully</returns>
    Task<bool> CancelAuthorizationAsync(
        string authorizationId,
        string cancelledBy,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed authorization
    /// </summary>
    /// <param name="authorizationId">Authorization identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authorization response</returns>
    Task<PaymentAuthorizationResponse> RetryAuthorizationAsync(
        string authorizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates payment request without creating authorization
    /// </summary>
    /// <param name="request">Payment authorization request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<PaymentValidationResult> ValidatePaymentAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of payment validation (pre-authorization check)
/// </summary>
public class PaymentValidationResult
{
    /// <summary>
    /// Indicates if validation passed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Estimated processing time in seconds
    /// </summary>
    public int EstimatedProcessingTimeSeconds { get; set; }

    /// <summary>
    /// Required validation systems
    /// </summary>
    public List<string> RequiredValidationSystems { get; set; } = new();
}
