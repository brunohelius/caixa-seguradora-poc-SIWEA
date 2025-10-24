using Microsoft.AspNetCore.Mvc;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Api.Controllers;

/// <summary>
/// Payment authorization endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentAuthorizationController : ControllerBase
{
    private readonly IPaymentAuthorizationService _authorizationService;
    private readonly ILogger<PaymentAuthorizationController> _logger;

    public PaymentAuthorizationController(
        IPaymentAuthorizationService authorizationService,
        ILogger<PaymentAuthorizationController> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Authorizes a payment request
    /// </summary>
    /// <param name="request">Payment authorization request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment authorization response</returns>
    /// <response code="200">Payment authorized successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("authorize-payment")]
    [ProducesResponseType(typeof(PaymentAuthorizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentAuthorizationResponse>> AuthorizePayment(
        [FromBody] PaymentAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing payment authorization for ClaimId: {ClaimId}", request.ClaimId);

            var response = await _authorizationService.AuthorizePaymentAsync(request, cancellationToken);

            if (response.Status == "REJECTED")
            {
                _logger.LogWarning("Payment authorization rejected: {AuthId} - {Errors}",
                    response.AuthorizationId, string.Join("; ", response.Errors));
            }
            else
            {
                _logger.LogInformation("Payment authorization processed: {AuthId} - Status: {Status}",
                    response.AuthorizationId, response.Status);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment authorization failed for ClaimId: {ClaimId}", request.ClaimId);
            return StatusCode(500, new { error = "Payment authorization failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets authorization status by ID
    /// </summary>
    /// <param name="authorizationId">Authorization identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authorization status</returns>
    /// <response code="200">Authorization found</response>
    /// <response code="404">Authorization not found</response>
    [HttpGet("{authorizationId}")]
    [ProducesResponseType(typeof(PaymentAuthorizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentAuthorizationResponse>> GetAuthorizationStatus(
        string authorizationId,
        CancellationToken cancellationToken)
    {
        var response = await _authorizationService.GetAuthorizationStatusAsync(authorizationId, cancellationToken);

        if (response == null)
        {
            return NotFound(new { error = $"Authorization {authorizationId} not found" });
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets all authorizations for a claim
    /// </summary>
    /// <param name="claimId">Claim identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of authorizations</returns>
    /// <response code="200">Authorizations retrieved successfully</response>
    [HttpGet("claim/{claimId}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentAuthorizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentAuthorizationResponse>>> GetClaimAuthorizations(
        int claimId,
        CancellationToken cancellationToken)
    {
        var authorizations = await _authorizationService.GetClaimAuthorizationsAsync(claimId, cancellationToken);
        return Ok(authorizations);
    }

    /// <summary>
    /// Validates a payment request without creating authorization
    /// </summary>
    /// <param name="request">Payment authorization request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    /// <response code="200">Validation completed</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(PaymentValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentValidationResult>> ValidatePayment(
        [FromBody] PaymentAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authorizationService.ValidatePaymentAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancels a pending authorization
    /// </summary>
    /// <param name="authorizationId">Authorization identifier</param>
    /// <param name="cancelRequest">Cancellation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation result</returns>
    /// <response code="200">Authorization cancelled</response>
    /// <response code="404">Authorization not found or cannot be cancelled</response>
    [HttpPost("{authorizationId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAuthorization(
        string authorizationId,
        [FromBody] CancelAuthorizationRequest cancelRequest,
        CancellationToken cancellationToken)
    {
        var success = await _authorizationService.CancelAuthorizationAsync(
            authorizationId,
            cancelRequest.CancelledBy,
            cancelRequest.Reason,
            cancellationToken);

        if (!success)
        {
            return NotFound(new { error = $"Authorization {authorizationId} not found or cannot be cancelled" });
        }

        return Ok(new { message = "Authorization cancelled successfully" });
    }

    /// <summary>
    /// Retries a failed authorization
    /// </summary>
    /// <param name="authorizationId">Authorization identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authorization response</returns>
    /// <response code="200">Authorization retried successfully</response>
    /// <response code="404">Authorization not found</response>
    [HttpPost("{authorizationId}/retry")]
    [ProducesResponseType(typeof(PaymentAuthorizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentAuthorizationResponse>> RetryAuthorization(
        string authorizationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authorizationService.RetryAuthorizationAsync(authorizationId, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request to cancel an authorization
/// </summary>
public class CancelAuthorizationRequest
{
    /// <summary>
    /// User cancelling the authorization
    /// </summary>
    public string CancelledBy { get; set; } = string.Empty;

    /// <summary>
    /// Cancellation reason
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
