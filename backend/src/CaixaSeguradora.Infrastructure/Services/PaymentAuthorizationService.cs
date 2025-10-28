using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Enums;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Core.ValueObjects;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// F02: Payment Authorization Service with 8-Step Authorization Pipeline (35 PF)
///
/// Pre-Transaction Validation (Steps 1-4):
///   1. Validate request (FluentValidation + business rules)
///   2. Search claim (ensure exists + retrieve related data)
///   3. External validation (ExternalServiceRouter → CNOUA/SIPUA/SIMDA)
///   4. Business rules validation (BR-004, BR-005, BR-019, BR-023, BR-067)
///
/// Transaction Pipeline (Steps 5-8) - Atomic with rollback:
///   5. Insert THISTSIN (ClaimHistory) - operation 1098, DTMOVABE, HORAOPER, EZEUSRID
///   6. Update TMESTSIN (ClaimMaster) - increment TOTPAG, increment OCORHIST
///   7. Insert SI_ACOMPANHA_SINI (ClaimAccompaniment) - COD_EVENTO=1098, operator ID
///   8. Update SI_SINISTRO_FASE (ClaimPhase) - open/close phases via PhaseManagementService
///
/// Critical Business Rules:
/// - BR-004: TipoPagamento ∈ {1,2,3,4,5}
/// - BR-005: ValorPrincipal <= (SDOPAG - TOTPAG)
/// - BR-019: Beneficiário required if TPSEGU != 0
/// - BR-023: Currency conversion VALPRIBT = VALPRI × VLCRUZAD
/// - BR-067: Transaction atomicity (all-or-nothing)
/// </summary>
public class PaymentAuthorizationService : IPaymentAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExternalServiceRouter _externalServiceRouter;
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IPhaseManagementService _phaseManagementService;
    private readonly IValidator<PaymentAuthorizationRequest> _validator;
    private readonly ILogger<PaymentAuthorizationService> _logger;

    public PaymentAuthorizationService(
        IUnitOfWork unitOfWork,
        ExternalServiceRouter externalServiceRouter,
        ICurrencyConversionService currencyConversionService,
        IPhaseManagementService phaseManagementService,
        IValidator<PaymentAuthorizationRequest> validator,
        ILogger<PaymentAuthorizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _externalServiceRouter = externalServiceRouter;
        _currencyConversionService = currencyConversionService;
        _phaseManagementService = phaseManagementService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Authorizes payment with 8-step pipeline (4 pre-transaction + 4 atomic transaction steps)
    /// </summary>
    public async Task<PaymentAuthorizationResponse> AuthorizePaymentAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationId = Guid.NewGuid();

        _logger.LogInformation(
            "F02 Payment Authorization START: {AuthId}, Claim={ClaimId}, Amount={Amount}",
            authorizationId, request.ClaimId, request.Amount);

        try
        {
            // ============================================================
            // PRE-TRANSACTION VALIDATION (Steps 1-4)
            // ============================================================

            // STEP 1: Validate request with FluentValidation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Step 1 FAILED: Validation errors: {Errors}", string.Join("; ", errors));
                return CreateErrorResponse(authorizationId, request, errors);
            }

            _logger.LogInformation("Step 1 PASSED: Request validation completed");

            // STEP 2: Search claim (ensure exists)
            var claim = await FindClaimAsync(request, cancellationToken);
            if (claim == null)
            {
                var error = $"Sinistro não encontrado: {request.Tipseg}/{request.Orgsin}/{request.Rmosin}/{request.Numsin}";
                _logger.LogWarning("Step 2 FAILED: {Error}", error);
                return CreateErrorResponse(authorizationId, request, new List<string> { error });
            }

            _logger.LogInformation(
                "Step 2 PASSED: Claim found - Protocol {Fonte}/{Protsini}-{Dac}",
                claim.Fonte, claim.Protsini, claim.Dac);

            // Get system control for business date
            var systemControl = await _unitOfWork.SystemControls.GetByIdAsync(new object[] { "SI" });
            if (systemControl == null)
            {
                var error = "Controle de sistema (TSISTEMA) não encontrado";
                _logger.LogError("Step 2 FAILED: {Error}", error);
                return CreateErrorResponse(authorizationId, request, new List<string> { error });
            }

            // STEP 3: External validation (if required)
            ExternalValidationResponse? externalValidation = null;
            if (request.ExternalValidationRequired && !request.BypassValidation)
            {
                var externalRequest = MapToExternalValidationRequest(request, claim);
                externalValidation = await _externalServiceRouter.RouteAndValidateAsync(
                    externalRequest, cancellationToken);

                if (!externalValidation.IsSuccess)
                {
                    var error = $"Validação externa falhou: {externalValidation.ErrorMessage ?? externalValidation.Ezert8}";
                    _logger.LogWarning("Step 3 FAILED: {Error}", error);
                    return CreateErrorResponse(authorizationId, request, new List<string> { error });
                }

                _logger.LogInformation(
                    "Step 3 PASSED: External validation succeeded - Service={Service}",
                    externalValidation.ValidationService);
            }
            else
            {
                _logger.LogInformation("Step 3 SKIPPED: External validation not required");
            }

            // STEP 4: Business rules validation
            var businessRuleErrors = ValidateBusinessRules(request, claim);
            if (businessRuleErrors.Any())
            {
                _logger.LogWarning("Step 4 FAILED: Business rules violated: {Errors}",
                    string.Join("; ", businessRuleErrors));
                return CreateErrorResponse(authorizationId, request, businessRuleErrors);
            }

            _logger.LogInformation("Step 4 PASSED: Business rules validation completed");

            // ============================================================
            // ATOMIC TRANSACTION PIPELINE (Steps 5-8)
            // ============================================================

            var context = new TransactionContext
            {
                AuthorizationId = authorizationId,
                ClaimKey = new ClaimKey(claim.Tipseg, claim.Orgsin, claim.Rmosin, claim.Numsin),
                OperatorId = request.RequestedBy,
                TransactionDate = systemControl.Dtmovabe,
                TransactionTime = TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks),
                OperationCode = 1098,
                CorrectionType = "5",
                Step = TransactionStep.History
            };

            if (!context.IsValid())
            {
                var error = "TransactionContext inválido";
                _logger.LogError("Transaction context validation failed");
                return CreateErrorResponse(authorizationId, request, new List<string> { error });
            }

            // Start EF Core transaction with ReadCommitted isolation
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // STEP 5: Insert THISTSIN (ClaimHistory)
                var history = await CreateHistoryRecordAsync(request, claim, context, cancellationToken);
                await _unitOfWork.ClaimHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                context.AdvanceStep(); // → ClaimMaster

                _logger.LogInformation(
                    "Step 5 COMPLETED: THISTSIN inserted - Occurrence={Occurrence}",
                    history.Ocorhist);

                // STEP 6: Update TMESTSIN (ClaimMaster)
                claim.Totpag += request.Amount;
                claim.Ocorhist++;
                claim.UpdatedBy = request.RequestedBy;
                claim.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                context.AdvanceStep(); // → Accompaniment

                _logger.LogInformation(
                    "Step 6 COMPLETED: TMESTSIN updated - TOTPAG={Totpag}, OCORHIST={Ocorhist}",
                    claim.Totpag, claim.Ocorhist);

                // STEP 7: Insert SI_ACOMPANHA_SINI (ClaimAccompaniment)
                var accompaniment = CreateAccompanimentRecord(claim, context);
                await _unitOfWork.ClaimAccompaniments.AddAsync(accompaniment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                context.AdvanceStep(); // → Phases

                _logger.LogInformation(
                    "Step 7 COMPLETED: SI_ACOMPANHA_SINI inserted - Event={Event}",
                    accompaniment.CodEvento);

                // STEP 8: Update SI_SINISTRO_FASE (ClaimPhase)
                await _phaseManagementService.UpdatePhasesAsync(
                    claim.Fonte,
                    claim.Protsini,
                    claim.Dac,
                    context.OperationCode,
                    context.TransactionDate,
                    context.OperatorId
                );
                context.AdvanceStep(); // → Committed

                _logger.LogInformation("Step 8 COMPLETED: SI_SINISTRO_FASE updated");

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "F02 Payment Authorization COMMITTED: {AuthId} - Transaction={TransactionId}",
                    authorizationId, context.AuthorizationId);

                return CreateSuccessResponse(
                    authorizationId,
                    request,
                    claim,
                    context,
                    history,
                    externalValidation);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                context.MarkForRollback($"Failed at step {context.Step}: {ex.Message}");

                _logger.LogError(ex,
                    "F02 Payment Authorization ROLLBACK: {AuthId} at step {Step}",
                    authorizationId, context.Step);

                return CreateRollbackResponse(authorizationId, request, context, ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "F02 Payment Authorization EXCEPTION: {AuthId}",
                authorizationId);

            return CreateErrorResponse(authorizationId, request,
                new List<string> { $"Erro do sistema: {ex.Message}" });
        }
    }

    #region Pre-Transaction Helpers

    private async Task<ClaimMaster?> FindClaimAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Tipseg.HasValue && request.Orgsin.HasValue &&
            request.Rmosin.HasValue && request.Numsin.HasValue)
        {
            var claims = await _unitOfWork.Claims.FindAsync(c =>
                c.Tipseg == request.Tipseg.Value &&
                c.Orgsin == request.Orgsin.Value &&
                c.Rmosin == request.Rmosin.Value &&
                c.Numsin == request.Numsin.Value);

            return claims.FirstOrDefault();
        }

        // Fallback to ClaimId lookup (if composite key not provided)
        // Note: ClaimMaster has composite key (Tipseg, Orgsin, Rmosin, Numsin)
        // ClaimId is a simplified identifier - would need mapping logic
        return null;
    }

    private ExternalValidationRequest MapToExternalValidationRequest(
        PaymentAuthorizationRequest request,
        ClaimMaster claim)
    {
        return new ExternalValidationRequest
        {
            Fonte = claim.Fonte,
            Protsini = claim.Protsini,
            Dac = claim.Dac,
            Orgsin = claim.Orgsin,
            Rmosin = claim.Rmosin,
            Numsin = claim.Numsin,
            CodProdu = claim.Codprodu,
            NumContrato = null, // TODO: Lookup from EF_CONTR_SEG_HABIT
            TipoPagamento = request.PaymentType,
            ValorPrincipal = request.Amount,
            ValorCorrecao = 0, // TODO: Calculate correction
            Beneficiario = request.BeneficiaryName,
            OperatorId = request.RequestedBy
        };
    }

    private List<string> ValidateBusinessRules(
        PaymentAuthorizationRequest request,
        ClaimMaster claim)
    {
        var errors = new List<string>();

        // BR-004: Payment type must be 1-5
        if (request.PaymentType < 1 || request.PaymentType > 5)
        {
            errors.Add("BR-004: Tipo de pagamento deve ser entre 1 e 5");
        }

        // BR-005: Amount must not exceed pending balance
        var pendingBalance = claim.Sdopag - claim.Totpag;
        if (request.Amount > pendingBalance)
        {
            errors.Add($"BR-005: Valor ({request.Amount:F2}) excede saldo pendente ({pendingBalance:F2})");
        }

        // BR-019: Beneficiary required if TPSEGU != 0
        if (claim.Tpsegu != 0 && string.IsNullOrWhiteSpace(request.BeneficiaryName))
        {
            errors.Add("BR-019: Beneficiário obrigatório quando TPSEGU != 0");
        }

        // BR-006: Policy type must be '1' or '2'
        if (claim.Tipreg != "1" && claim.Tipreg != "2")
        {
            errors.Add($"BR-006: Tipo de apólice inválido: {claim.Tipreg} (deve ser '1' ou '2')");
        }

        return errors;
    }

    #endregion

    #region Transaction Step Helpers

    private async Task<ClaimHistory> CreateHistoryRecordAsync(
        PaymentAuthorizationRequest request,
        ClaimMaster claim,
        TransactionContext context,
        CancellationToken cancellationToken)
    {
        // BR-023: Currency conversion
        var conversionResult = await _currencyConversionService.ConvertAsync(
            request.Amount,
            request.CurrencyCode,
            "BTNF",
            cancellationToken);

        if (!conversionResult.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Currency conversion failed: {conversionResult.ErrorMessage}");
        }

        var timeString = context.TransactionTime.ToString(@"hhmmss");

        return new ClaimHistory
        {
            Tipseg = claim.Tipseg,
            Orgsin = claim.Orgsin,
            Rmosin = claim.Rmosin,
            Numsin = claim.Numsin,
            Ocorhist = claim.Ocorhist + 1, // Next occurrence
            Operacao = context.OperationCode, // 1098
            Dtmovto = context.TransactionDate,
            Horaoper = timeString,
            Valpri = request.Amount,
            Crrmon = 0, // TODO: Calculate correction
            Nomfav = request.BeneficiaryName,
            Tipcrr = context.CorrectionType, // "5"
            Valpribt = conversionResult.ConvertedAmount,
            Crrmonbt = 0,
            Valtotbt = conversionResult.ConvertedAmount,
            Sitcontb = "0",
            Situacao = "0",
            Ezeusrid = context.OperatorId,
            CreatedBy = context.OperatorId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ClaimAccompaniment CreateAccompanimentRecord(
        ClaimMaster claim,
        TransactionContext context)
    {
        var timeString = context.TransactionTime.ToString(@"hhmmss");

        return new ClaimAccompaniment
        {
            Fonte = claim.Fonte,
            Protsini = claim.Protsini,
            Dac = claim.Dac,
            CodEvento = context.OperationCode, // 1098
            DataMovtoSiniaco = context.TransactionDate,
            NumOcorrSiniaco = claim.Ocorhist,
            DescrComplementar = "Autorização de pagamento",
            CodUsuario = context.OperatorId,
            HoraEvento = timeString,
            NomeEvento = "PAGAMENTO_AUTORIZADO",
            CreatedBy = context.OperatorId,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Response Creation

    private PaymentAuthorizationResponse CreateSuccessResponse(
        Guid authorizationId,
        PaymentAuthorizationRequest request,
        ClaimMaster claim,
        TransactionContext context,
        ClaimHistory history,
        ExternalValidationResponse? externalValidation)
    {
        var response = new PaymentAuthorizationResponse
        {
            AuthorizationId = authorizationId.ToString(),
            Status = "APPROVED",
            ClaimId = request.ClaimId,
            AuthorizedAmount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            AuthorizedAt = DateTime.UtcNow,
            AuthorizedBy = request.RequestedBy,
            TransactionId = context.AuthorizationId,
            TransactionDate = context.TransactionDate,
            TransactionTime = context.TransactionTime.ToString(@"hhmmss"),
            HistoryOccurrence = history.Ocorhist,
            TransactionReference = $"TXN-{authorizationId:N}",
            RollbackOccurred = false,
            FailedStep = null
        };

        if (externalValidation != null)
        {
            response.ExternalValidationResults.Add(new ExternalValidationSummary
            {
                ServiceName = externalValidation.ValidationService,
                Status = "APPROVED",
                Message = "Validação externa aprovada",
                ResponseTimeMs = externalValidation.ElapsedMilliseconds
            });
        }

        return response;
    }

    private PaymentAuthorizationResponse CreateErrorResponse(
        Guid authorizationId,
        PaymentAuthorizationRequest request,
        List<string> errors)
    {
        return new PaymentAuthorizationResponse
        {
            AuthorizationId = authorizationId.ToString(),
            Status = "REJECTED",
            ClaimId = request.ClaimId,
            AuthorizedAmount = 0,
            CurrencyCode = request.CurrencyCode,
            AuthorizedAt = DateTime.UtcNow,
            AuthorizedBy = "SYSTEM_VALIDATION",
            Errors = errors,
            RollbackOccurred = false
        };
    }

    private PaymentAuthorizationResponse CreateRollbackResponse(
        Guid authorizationId,
        PaymentAuthorizationRequest request,
        TransactionContext context,
        Exception ex)
    {
        return new PaymentAuthorizationResponse
        {
            AuthorizationId = authorizationId.ToString(),
            Status = "REJECTED",
            ClaimId = request.ClaimId,
            AuthorizedAmount = 0,
            CurrencyCode = request.CurrencyCode,
            AuthorizedAt = DateTime.UtcNow,
            AuthorizedBy = "SYSTEM_ROLLBACK",
            Errors = new List<string> { context.RollbackReason ?? ex.Message },
            RollbackOccurred = true,
            FailedStep = context.Step.ToString(),
            TransactionId = context.AuthorizationId
        };
    }

    #endregion

    #region Stub Methods (not used in new implementation)

    public Task<PaymentAuthorizationResponse?> GetAuthorizationStatusAsync(
        string authorizationId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement query from database
        return Task.FromResult<PaymentAuthorizationResponse?>(null);
    }

    public Task<IEnumerable<PaymentAuthorizationResponse>> GetClaimAuthorizationsAsync(
        int claimId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement query from database
        return Task.FromResult<IEnumerable<PaymentAuthorizationResponse>>(
            Array.Empty<PaymentAuthorizationResponse>());
    }

    public Task<bool> CancelAuthorizationAsync(
        string authorizationId,
        string cancelledBy,
        string reason,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement cancellation logic
        return Task.FromResult(false);
    }

    public Task<PaymentAuthorizationResponse> RetryAuthorizationAsync(
        string authorizationId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Retry logic not yet implemented");
    }

    public Task<PaymentValidationResult> ValidatePaymentAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement validation-only logic
        return Task.FromResult(new PaymentValidationResult
        {
            IsValid = true,
            Errors = new List<string>(),
            EstimatedProcessingTimeSeconds = 5,
            RequiredValidationSystems = new List<string>()
        });
    }

    #endregion
}
