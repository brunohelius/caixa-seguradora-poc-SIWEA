using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Service for accessing legacy system documentation and metrics
/// </summary>
public interface ILegacySystemDocsService
{
    /// <summary>
    /// Gets comprehensive dashboard data for the legacy system
    /// </summary>
    Task<LegacySystemDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets content of a specific documentation file
    /// </summary>
    Task<string> GetDocumentContentAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets list of all available documentation files
    /// </summary>
    Task<List<DocumentDto>> GetAvailableDocumentsAsync(CancellationToken cancellationToken = default);
}
