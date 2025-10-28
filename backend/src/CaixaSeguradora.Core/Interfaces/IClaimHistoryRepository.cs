using CaixaSeguradora.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// T085 [US3] - Specialized repository for ClaimHistory with optimized pagination
/// Prevents N+1 queries and leverages database indexing
/// </summary>
public interface IClaimHistoryRepository : IRepository<ClaimHistory>
{
    /// <summary>
    /// Get paginated claim history with optimized query execution
    /// Uses recommended index: IX_THISTSIN_Claim_Occurrence
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple with total count and paginated history records</returns>
    Task<(int TotalCount, List<ClaimHistory> Records)> GetPaginatedHistoryAsync(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of history records for a specific claim
    /// Optimized for index usage
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total history count</returns>
    Task<int> GetHistoryCountAsync(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        CancellationToken cancellationToken = default);
}
