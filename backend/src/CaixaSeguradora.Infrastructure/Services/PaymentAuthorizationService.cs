using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using FluentValidation;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// Payment authorization service with multi-system validation
/// </summary>
public class PaymentAuthorizationService : IPaymentAuthorizationService
{
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IEnumerable<IExternalValidationService> _validationServices;
    private readonly IPhaseManagementService _phaseManagementService;
    private readonly IValidator<PaymentAuthorizationRequest> _validator;
    private readonly ILogger<PaymentAuthorizationService> _logger;

    // In-memory storage (replace with database in production)
    private static readonly ConcurrentDictionary<string, PaymentAuthorizationResponse> _authorizations = new();
    private static readonly ConcurrentDictionary<int, List<PaymentAuthorizationResponse>> _claimAuthorizations = new();

    public PaymentAuthorizationService(
        ICurrencyConversionService currencyConversionService,
        IEnumerable<IExternalValidationService> validationServices,
        IPhaseManagementService phaseManagementService,
        IValidator<PaymentAuthorizationRequest> validator,
        ILogger<PaymentAuthorizationService> logger)
    {
        _currencyConversionService = currencyConversionService;
        _validationServices = validationServices;
        _phaseManagementService = phaseManagementService;
        _validator = validator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaymentAuthorizationResponse> AuthorizePaymentAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationId = GenerateAuthorizationId();

        try
        {
            _logger.LogInformation("Starting payment authorization: {AuthId} for ClaimId: {ClaimId}",
                authorizationId, request.ClaimId);

            // Step 1: Validate request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Payment authorization validation failed: {Errors}", string.Join("; ", errors));

                var errorResponse = CreateErrorResponse(authorizationId, request, errors);
                StoreAuthorization(errorResponse);
                return errorResponse;
            }

            // Step 2: Currency conversion (if needed)
            var targetCurrency = request.TargetCurrencyCode ?? request.CurrencyCode;
            var finalAmount = request.Amount;
            decimal? exchangeRate = null;

            if (!request.CurrencyCode.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
            {
                var conversionResult = await _currencyConversionService.ConvertAsync(
                    request.Amount, request.CurrencyCode, targetCurrency, cancellationToken);

                if (!conversionResult.IsSuccess)
                {
                    var errorResponse = CreateErrorResponse(authorizationId, request,
                        new List<string> { $"Currency conversion failed: {conversionResult.ErrorMessage}" });
                    StoreAuthorization(errorResponse);
                    return errorResponse;
                }

                finalAmount = conversionResult.ConvertedAmount;
                exchangeRate = conversionResult.ExchangeRate;

                _logger.LogInformation("Currency converted: {Source} {Amount} -> {Target} {FinalAmount} (rate: {Rate})",
                    request.CurrencyCode, request.Amount, targetCurrency, finalAmount, exchangeRate);
            }

            // Step 3: External validation (parallel execution)
            var validationTasks = new List<Task<ValidationResult>>();

            if (!request.BypassValidation)
            {
                var requiredServices = DetermineRequiredValidationServices(request.ProductCode);

                foreach (var service in requiredServices)
                {
                    validationTasks.Add(ExecuteValidationAsync(service, request, cancellationToken));
                }

                _logger.LogInformation("Executing {Count} validation services for product: {Product}",
                    validationTasks.Count, request.ProductCode);
            }
            else
            {
                _logger.LogWarning("Validation bypassed for authorization: {AuthId}", authorizationId);
            }

            // Step 4: Wait for all validations
            var validationResults = validationTasks.Count > 0
                ? await Task.WhenAll(validationTasks)
                : Array.Empty<ValidationResult>();

            // Step 5: Determine overall status
            var hasRejections = validationResults.Any(v => v.Status == "REJECTED");
            var hasErrors = validationResults.Any(v => v.Status == "ERROR");
            var hasPending = validationResults.Any(v => v.Status == "PENDING");

            string overallStatus;
            if (hasRejections || hasErrors)
                overallStatus = "REJECTED";
            else if (hasPending)
                overallStatus = "PENDING";
            else
                overallStatus = "APPROVED";

            // Step 6: Create response
            var response = new PaymentAuthorizationResponse
            {
                AuthorizationId = authorizationId,
                Status = overallStatus,
                ClaimId = request.ClaimId,
                AuthorizedAmount = finalAmount,
                CurrencyCode = targetCurrency,
                ExchangeRate = exchangeRate,
                OriginalAmount = request.Amount,
                OriginalCurrencyCode = request.CurrencyCode,
                AuthorizedAt = DateTime.UtcNow,
                AuthorizedBy = overallStatus == "APPROVED" ? "SYSTEM_AUTO" : "PENDING_REVIEW",
                ValidationResults = validationResults.ToList(),
                Errors = validationResults.Where(v => v.Status == "ERROR" || v.Status == "REJECTED")
                    .Select(v => v.Message).ToList(),
                Warnings = validationResults.Where(v => v.AdditionalData?.ContainsKey("warnings") == true)
                    .SelectMany(v => v.AdditionalData!["warnings"] as List<string> ?? new List<string>())
                    .ToList(),
                TransactionReference = $"TXN-{authorizationId}",
                EstimatedProcessingTime = overallStatus == "PENDING" ? 300 : null,
                NextAction = overallStatus == "PENDING" ? "MANUAL_REVIEW_REQUIRED" : null,
                ExpiresAt = overallStatus == "PENDING" ? DateTime.UtcNow.AddHours(24) : null
            };

            // Step 7: Store authorization
            StoreAuthorization(response);

            // T098 [US5]: Update phases after successful payment authorization
            // Note: Phase integration requires claim protocol information
            // This would be implemented in the full PaymentAuthorizationService
            // with proper claim lookup and phase event tracking
            if (overallStatus == "APPROVED")
            {
                _logger.LogInformation(
                    "Payment authorization {AuthId} approved. Phase tracking integration point.",
                    authorizationId);
                // TODO: Implement phase tracking integration when claim protocol is available
                // await _phaseManagementService.UpdatePhasesAsync(...)
            }

            _logger.LogInformation("Payment authorization completed: {AuthId} - Status: {Status}",
                authorizationId, overallStatus);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment authorization failed: {AuthId}", authorizationId);

            var errorResponse = CreateErrorResponse(authorizationId, request,
                new List<string> { $"System error: {ex.Message}" });
            StoreAuthorization(errorResponse);
            return errorResponse;
        }
    }

    /// <inheritdoc/>
    public Task<PaymentAuthorizationResponse?> GetAuthorizationStatusAsync(
        string authorizationId,
        CancellationToken cancellationToken = default)
    {
        _authorizations.TryGetValue(authorizationId, out var response);
        return Task.FromResult(response);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<PaymentAuthorizationResponse>> GetClaimAuthorizationsAsync(
        int claimId,
        CancellationToken cancellationToken = default)
    {
        _claimAuthorizations.TryGetValue(claimId, out var authorizations);
        return Task.FromResult<IEnumerable<PaymentAuthorizationResponse>>(authorizations ?? new List<PaymentAuthorizationResponse>());
    }

    /// <inheritdoc/>
    public Task<bool> CancelAuthorizationAsync(
        string authorizationId,
        string cancelledBy,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (_authorizations.TryGetValue(authorizationId, out var authorization))
        {
            if (authorization.Status == "PENDING")
            {
                authorization.Status = "CANCELLED";
                authorization.Warnings.Add($"Cancelled by {cancelledBy}: {reason}");

                _logger.LogInformation("Authorization cancelled: {AuthId} by {User}", authorizationId, cancelledBy);
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public async Task<PaymentAuthorizationResponse> RetryAuthorizationAsync(
        string authorizationId,
        CancellationToken cancellationToken = default)
    {
        if (_authorizations.TryGetValue(authorizationId, out var originalAuth))
        {
            var retryRequest = new PaymentAuthorizationRequest
            {
                ClaimId = originalAuth.ClaimId,
                Amount = originalAuth.OriginalAmount ?? originalAuth.AuthorizedAmount,
                CurrencyCode = originalAuth.OriginalCurrencyCode ?? originalAuth.CurrencyCode,
                RequestedBy = $"RETRY_{originalAuth.AuthorizedBy}"
            };

            return await AuthorizePaymentAsync(retryRequest, cancellationToken);
        }

        throw new InvalidOperationException($"Authorization {authorizationId} not found");
    }

    /// <inheritdoc/>
    public async Task<PaymentValidationResult> ValidatePaymentAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        var result = new PaymentValidationResult
        {
            IsValid = validationResult.IsValid,
            Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
            EstimatedProcessingTimeSeconds = 5,
            RequiredValidationSystems = DetermineRequiredValidationServices(request.ProductCode)
                .Select(s => s.SystemName).ToList()
        };

        return result;
    }

    private async Task<ValidationResult> ExecuteValidationAsync(
        IExternalValidationService service,
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationRequest = new ExternalValidationRequest
            {
                ClaimId = request.ClaimId,
                ProductCode = request.ProductCode,
                Amount = request.Amount,
                CurrencyCode = request.CurrencyCode,
                BeneficiaryTaxId = request.BeneficiaryTaxId,
                PaymentMethod = request.PaymentMethod,
                RequestedAt = request.RequestedAt
            };

            var response = await service.ValidatePaymentAsync(validationRequest, cancellationToken);

            return new ValidationResult
            {
                SystemName = service.SystemName,
                Status = response.Status,
                Message = response.Message,
                ValidatedAt = response.ValidatedAt,
                ResponseTimeMs = response.ResponseTimeMs,
                AdditionalData = response.AdditionalData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation service {Service} failed", service.SystemName);

            return new ValidationResult
            {
                SystemName = service.SystemName,
                Status = "ERROR",
                Message = $"Validation service error: {ex.Message}",
                ValidatedAt = DateTime.UtcNow,
                ResponseTimeMs = 0
            };
        }
    }

    private IEnumerable<IExternalValidationService> DetermineRequiredValidationServices(string productCode)
    {
        var required = new List<IExternalValidationService>();

        foreach (var service in _validationServices)
        {
            // Simple routing logic based on product code prefix
            if (productCode.StartsWith("CNOUA", StringComparison.OrdinalIgnoreCase) && service.SystemName == "CNOUA")
                required.Add(service);
            else if (productCode.StartsWith("SIPUA", StringComparison.OrdinalIgnoreCase) && service.SystemName == "SIPUA")
                required.Add(service);
            else if (productCode.StartsWith("SIMDA", StringComparison.OrdinalIgnoreCase) && service.SystemName == "SIMDA")
                required.Add(service);
        }

        // Default: use all services if no specific routing
        return required.Any() ? required : _validationServices;
    }

    private void StoreAuthorization(PaymentAuthorizationResponse response)
    {
        _authorizations[response.AuthorizationId] = response;

        _claimAuthorizations.AddOrUpdate(
            response.ClaimId,
            new List<PaymentAuthorizationResponse> { response },
            (key, existing) =>
            {
                existing.Add(response);
                return existing;
            });
    }

    private PaymentAuthorizationResponse CreateErrorResponse(
        string authorizationId,
        PaymentAuthorizationRequest request,
        List<string> errors)
    {
        return new PaymentAuthorizationResponse
        {
            AuthorizationId = authorizationId,
            Status = "REJECTED",
            ClaimId = request.ClaimId,
            AuthorizedAmount = 0,
            CurrencyCode = request.CurrencyCode,
            AuthorizedAt = DateTime.UtcNow,
            AuthorizedBy = "SYSTEM_VALIDATION",
            Errors = errors
        };
    }

    private string GenerateAuthorizationId()
    {
        return $"AUTH-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
