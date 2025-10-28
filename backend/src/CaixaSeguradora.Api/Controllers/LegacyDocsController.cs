using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CaixaSeguradora.Api.Controllers;

/// <summary>
/// Controller for accessing legacy system documentation
/// Provides endpoints for viewing documentation dashboard and individual documents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LegacyDocsController : ControllerBase
{
    private readonly ILegacySystemDocsService _docsService;
    private readonly ILogger<LegacyDocsController> _logger;

    public LegacyDocsController(
        ILegacySystemDocsService docsService,
        ILogger<LegacyDocsController> logger)
    {
        _docsService = docsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets comprehensive dashboard data for the legacy system
    /// </summary>
    /// <returns>Complete dashboard with system info, statistics, and documentation metrics</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(LegacySystemDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LegacySystemDashboardDto>> GetDashboard(
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching legacy system dashboard data");
            var dashboard = await _docsService.GetDashboardDataAsync(cancellationToken);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching legacy system dashboard");
            return StatusCode(500, new
            {
                Message = "Erro ao carregar dados da documentação do sistema legado",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Gets list of all available documentation files
    /// </summary>
    /// <returns>List of documentation documents with metadata</returns>
    [HttpGet("documents")]
    [ProducesResponseType(typeof(List<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DocumentDto>>> GetDocuments(
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching available documents list");
            var documents = await _docsService.GetAvailableDocumentsAsync(cancellationToken);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents list");
            return StatusCode(500, new
            {
                Message = "Erro ao carregar lista de documentos",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Gets content of a specific documentation file
    /// </summary>
    /// <param name="documentId">Document identifier (e.g., "01-executive-summary")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw markdown content of the document</returns>
    [HttpGet("documents/{documentId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetDocumentContent(
        string documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching content for document {DocumentId}", documentId);
            var content = await _docsService.GetDocumentContentAsync(documentId, cancellationToken);
            return Ok(new { content });
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("Document {DocumentId} not found", documentId);
            return NotFound(new
            {
                Message = $"Documento '{documentId}' não encontrado"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document {DocumentId}", documentId);
            return StatusCode(500, new
            {
                Message = "Erro ao carregar conteúdo do documento",
                Details = ex.Message
            });
        }
    }
}
