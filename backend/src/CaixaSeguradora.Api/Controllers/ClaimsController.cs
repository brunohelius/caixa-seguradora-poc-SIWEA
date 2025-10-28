using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Exceptions;
using CaixaSeguradora.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CaixaSeguradora.Api.Controllers;

/// <summary>
/// Claims Controller
/// REST API endpoints for claim management
/// </summary>
[ApiController]
[Route("api/claims")]
[Produces("application/json")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;
    private readonly IPaymentAuthorizationService _paymentAuthorizationService;
    private readonly IPhaseManagementService _phaseManagementService;
    private readonly ILogger<ClaimsController> _logger;
    private readonly IValidator<ClaimSearchCriteria> _searchValidator;
    private readonly IValidator<PaymentAuthorizationRequest> _authorizationValidator;

    public ClaimsController(
        IClaimService claimService,
        IPaymentAuthorizationService paymentAuthorizationService,
        IPhaseManagementService phaseManagementService,
        ILogger<ClaimsController> logger,
        IValidator<ClaimSearchCriteria> searchValidator,
        IValidator<PaymentAuthorizationRequest> authorizationValidator)
    {
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _paymentAuthorizationService = paymentAuthorizationService ?? throw new ArgumentNullException(nameof(paymentAuthorizationService));
        _phaseManagementService = phaseManagementService ?? throw new ArgumentNullException(nameof(phaseManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _searchValidator = searchValidator ?? throw new ArgumentNullException(nameof(searchValidator));
        _authorizationValidator = authorizationValidator ?? throw new ArgumentNullException(nameof(authorizationValidator));
    }

    /// <summary>
    /// Search for a claim using protocol, claim number, or leader code
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <returns>Claim details if found</returns>
    /// <response code="200">Claim found successfully</response>
    /// <response code="400">Invalid search criteria</response>
    /// <response code="404">Claim not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ClaimSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchClaim([FromBody] ClaimSearchCriteria request)
    {
        var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogInformation("SearchClaim request received. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            // Validate request
            var validationResult = await _searchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Validation failed. Errors: {Errors}", string.Join(", ", errors));

                return BadRequest(new ErrorResponse
                {
                    Sucesso = false,
                    CodigoErro = "CRITERIOS_INVALIDOS",
                    Mensagem = "Critérios de busca inválidos",
                    Detalhes = errors,
                    Timestamp = DateTime.UtcNow,
                    TraceId = correlationId
                });
            }

            // Search claim
            var claim = await _claimService.SearchClaimAsync(request);

            return Ok(new ClaimSearchResponse
            {
                Sucesso = true,
                Sinistro = claim,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (ClaimNotFoundException ex)
        {
            _logger.LogWarning(ex, "Claim not found. CorrelationId: {CorrelationId}", correlationId);

            return NotFound(new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = ex.ErrorCode,
                Mensagem = ex.Message,
                Detalhes = new List<string>(),
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching claim. CorrelationId: {CorrelationId}", correlationId);

            return StatusCode(500, new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = "ERRO_INTERNO",
                Mensagem = "Erro ao buscar sinistro. Tente novamente mais tarde.",
                Detalhes = new List<string>(),
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
    }

    /// <summary>
    /// F02 [35 PF] - Authorize payment for a claim with external validation
    /// Implements 8-step authorization pipeline with transaction atomicity (BR-067)
    /// </summary>
    /// <param name="request">Payment authorization request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authorization response with transaction details</returns>
    /// <response code="200">Payment authorized successfully</response>
    /// <response code="400">Validation failed or business rule violation</response>
    /// <response code="500">Internal error (includes transaction rollback)</response>
    [HttpPost("authorize-payment")]
    [ProducesResponseType(typeof(PaymentAuthorizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AuthorizePayment(
        [FromBody] PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogInformation("AuthorizePayment request received. CorrelationId: {CorrelationId}, Claim: {Tipseg}/{Orgsin}/{Rmosin}/{Numsin}, Amount: {Amount}",
            correlationId, request.Tipseg, request.Orgsin, request.Rmosin, request.Numsin, request.Amount);

        try
        {
            // Step 1: FluentValidation request validation
            var validationResult = await _authorizationValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => $"{e.ErrorCode}: {e.ErrorMessage}").ToList();
                _logger.LogWarning("Authorization validation failed. Errors: {Errors}", string.Join(", ", errors));

                return BadRequest(new ErrorResponse
                {
                    Sucesso = false,
                    CodigoErro = "VALIDATION_FAILED",
                    Mensagem = "Requisição de autorização inválida",
                    Detalhes = errors,
                    Timestamp = DateTime.UtcNow,
                    TraceId = correlationId
                });
            }

            // Step 2-8: Execute authorization pipeline
            var response = await _paymentAuthorizationService.AuthorizePaymentAsync(request, cancellationToken);

            // Check result
            if (!response.Sucesso)
            {
                // BR-067: Transaction rollback occurred
                if (response.RollbackOccurred)
                {
                    _logger.LogError("Transaction rollback at step {FailedStep}. AuthId: {AuthId}",
                        response.FailedStep, response.AuthorizationId);

                    return StatusCode(500, new ErrorResponse
                    {
                        Sucesso = false,
                        CodigoErro = "TRANSACTION_ROLLBACK",
                        Mensagem = $"Falha na transação (passo {response.FailedStep}). Todas as alterações foram revertidas.",
                        Detalhes = response.Errors,
                        Timestamp = DateTime.UtcNow,
                        TraceId = correlationId
                    });
                }

                // Business rule violation or external validation failure
                _logger.LogWarning("Authorization failed without rollback. Errors: {Errors}",
                    string.Join(", ", response.Errors));

                return BadRequest(new ErrorResponse
                {
                    Sucesso = false,
                    CodigoErro = "AUTHORIZATION_FAILED",
                    Mensagem = response.Errors.FirstOrDefault() ?? "Falha na autorização de pagamento",
                    Detalhes = response.Errors,
                    Timestamp = DateTime.UtcNow,
                    TraceId = correlationId
                });
            }

            // Success
            _logger.LogInformation("Payment authorized successfully. AuthId: {AuthId}, HistoryOccurrence: {Occurrence}",
                response.AuthorizationId, response.HistoryOccurrence);

            return Ok(response);
        }
        catch (ClaimNotFoundException ex)
        {
            _logger.LogWarning(ex, "Claim not found for authorization. CorrelationId: {CorrelationId}", correlationId);

            return NotFound(new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = ex.ErrorCode,
                Mensagem = ex.Message,
                Detalhes = new List<string>(),
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during payment authorization. CorrelationId: {CorrelationId}", correlationId);

            return StatusCode(500, new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = "INTERNAL_ERROR",
                Mensagem = "Erro interno ao processar autorização. Tente novamente mais tarde.",
                Detalhes = new List<string>(),
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
    }

    /// <summary>
    /// Get claim by composite primary key
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <returns>Claim details</returns>
    /// <response code="200">Claim found successfully</response>
    /// <response code="404">Claim not found</response>
    [HttpGet("{tipseg}/{orgsin}/{rmosin}/{numsin}")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "tipseg", "orgsin", "rmosin", "numsin" })]  // T142: Cache for 60s
    [ProducesResponseType(typeof(ClaimDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimById(
        [FromRoute] int tipseg,
        [FromRoute] int orgsin,
        [FromRoute] int rmosin,
        [FromRoute] int numsin)
    {
        var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogInformation("GetClaimById: {Tipseg}/{Orgsin}/{Rmosin}/{Numsin}. CorrelationId: {CorrelationId}",
            tipseg, orgsin, rmosin, numsin, correlationId);

        try
        {
            // Validate parameters
            if (tipseg < 1 || orgsin < 1 || rmosin < 1 || numsin < 1)
            {
                return BadRequest(new ErrorResponse
                {
                    Sucesso = false,
                    CodigoErro = "PARAMETROS_INVALIDOS",
                    Mensagem = "Todos os parâmetros devem ser maiores que zero",
                    Detalhes = new List<string>(),
                    Timestamp = DateTime.UtcNow,
                    TraceId = correlationId
                });
            }

            var claim = await _claimService.GetClaimByIdAsync(tipseg, orgsin, rmosin, numsin);

            return Ok(new ClaimDetailResponse
            {
                Sucesso = true,
                Sinistro = claim,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (ClaimNotFoundException ex)
        {
            _logger.LogWarning(ex, "Claim not found. CorrelationId: {CorrelationId}", correlationId);

            return NotFound(new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = ex.ErrorCode,
                Mensagem = ex.Message,
                Detalhes = new List<string>(),
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claim. CorrelationId: {CorrelationId}", correlationId);

            return StatusCode(500, new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = "ERRO_INTERNO",
                Mensagem = "Erro ao obter sinistro. Tente novamente mais tarde.",
                Detalhes = new List<string>(),
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
    }

    /// <summary>
    /// T076 [US3] - Get claim payment history with pagination
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20, max 100)</param>
    /// <returns>Paginated claim history</returns>
    [HttpGet("{tipseg}/{orgsin}/{rmosin}/{numsin}/history")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ClaimHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimHistory(
        [FromRoute] int tipseg,
        [FromRoute] int orgsin,
        [FromRoute] int rmosin,
        [FromRoute] int numsin,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GetClaimHistory request for {Tipseg}/{Orgsin}/{Rmosin}/{Numsin}, Page: {Page}, PageSize: {PageSize}",
            tipseg, orgsin, rmosin, numsin, page, pageSize);

        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, page, pageSize);

            if (result == null || !result.Historico.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Sucesso = false,
                    CodigoErro = "HISTORICO_NAO_ENCONTRADO",
                    Mensagem = "Nenhum registro de histórico encontrado para este sinistro",
                    Timestamp = DateTime.UtcNow,
                    TraceId = correlationId
                });
            }

            return Ok(result);
        }
        catch (ClaimNotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = ex.ErrorCode,
                Mensagem = ex.Message,
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching claim history. CorrelationId: {CorrelationId}", correlationId);

            return StatusCode(500, new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = "ERRO_INTERNO",
                Mensagem = "Erro ao buscar histórico. Tente novamente mais tarde.",
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
    }

    /// <summary>
    /// T099 [US5] - Get claim phases with opening/closing tracking
    /// </summary>
    /// <param name="fonte">Protocol Source</param>
    /// <param name="protsini">Protocol Number</param>
    /// <param name="dac">Protocol Check Digit</param>
    /// <returns>List of phases with status</returns>
    [HttpGet("{fonte}/{protsini}/{dac}/phases")]
    [ProducesResponseType(typeof(PhaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClaimPhases(
        [FromRoute] int fonte,
        [FromRoute] int protsini,
        [FromRoute] int dac)
    {
        var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GetClaimPhases request for protocol {Fonte}/{Protsini}-{Dac}",
            fonte, protsini, dac);

        try
        {
            // Get all phases from service
            var phases = await _phaseManagementService.GetAllPhasesAsync(fonte, protsini, dac);

            if (!phases.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Sucesso = false,
                    CodigoErro = "FASES_NAO_ENCONTRADAS",
                    Mensagem = "Nenhuma fase encontrada para este sinistro",
                    Timestamp = DateTime.UtcNow,
                    TraceId = correlationId
                });
            }

            // Map to DTOs
            var phaseDtos = phases.Select(p => new PhaseRecordDto
            {
                CodFase = p.CodFase,
                NomeFase = $"Fase {p.CodFase}", // TODO: Add lookup table for phase names
                CodEvento = p.CodEvento,
                NomeEvento = $"Evento {p.CodEvento}", // TODO: Add lookup table for event names
                NumOcorrSiniaco = p.NumOcorrSiniaco,
                DataInivigRefaev = p.DataInivigRefaev,
                DataAberturaSifa = p.DataAberturaSifa,
                DataFechaSifa = p.DataFechaSifa
            }).OrderByDescending(p => p.DataAberturaSifa).ToList();

            var response = new PhaseResponse
            {
                Sucesso = true,
                Protocolo = $"{fonte:D3}/{protsini:D7}-{dac}",
                TotalFases = phaseDtos.Count,
                Fases = phaseDtos
            };

            _logger.LogInformation("Retrieved {Count} phases for protocol {Protocolo}",
                phaseDtos.Count, response.Protocolo);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching claim phases. CorrelationId: {CorrelationId}", correlationId);

            return StatusCode(500, new ErrorResponse
            {
                Sucesso = false,
                CodigoErro = "ERRO_INTERNO",
                Mensagem = "Erro ao buscar fases. Tente novamente mais tarde.",
                Timestamp = DateTime.UtcNow,
                TraceId = correlationId
            });
        }
    }
}

/// <summary>
/// Claim Search Response
/// </summary>
public class ClaimSearchResponse
{
    public bool Sucesso { get; set; }
    public ClaimDetailDto? Sinistro { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Claim Detail Response
/// </summary>
public class ClaimDetailResponse
{
    public bool Sucesso { get; set; }
    public ClaimDetailDto? Sinistro { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Error Response
/// </summary>
public class ErrorResponse
{
    public bool Sucesso { get; set; }
    public string CodigoErro { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public List<string> Detalhes { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
