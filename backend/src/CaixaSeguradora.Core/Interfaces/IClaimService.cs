using CaixaSeguradora.Core.DTOs;
using System.Threading.Tasks;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Claim Service Interface
/// Business logic operations for claim management
/// </summary>
public interface IClaimService
{
    /// <summary>
    /// Search for a claim using provided criteria
    /// Supports three search strategies: protocol, claim number, or leader code
    /// </summary>
    /// <param name="criteria">Search criteria with at least one complete set</param>
    /// <returns>Claim details if found</returns>
    /// <exception cref="ClaimNotFoundException">Thrown when no claim matches the criteria</exception>
    Task<ClaimDetailDto> SearchClaimAsync(ClaimSearchCriteria criteria);

    /// <summary>
    /// Get claim by composite primary key
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <returns>Claim details if found</returns>
    /// <exception cref="ClaimNotFoundException">Thrown when claim does not exist</exception>
    Task<ClaimDetailDto> GetClaimByIdAsync(int tipseg, int orgsin, int rmosin, int numsin);

    /// <summary>
    /// Validates if a claim exists using provided criteria
    /// </summary>
    /// <param name="criteria">Search criteria</param>
    /// <returns>True if claim exists, false otherwise</returns>
    Task<bool> ValidateClaimExistsAsync(ClaimSearchCriteria criteria);

    /// <summary>
    /// Gets the pending value for a claim
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <returns>Pending value (Sdopag - Totpag)</returns>
    Task<decimal> GetPendingValueAsync(int tipseg, int orgsin, int rmosin, int numsin);

    /// <summary>
    /// Get paginated claim history
    /// T078 [US3] - Implementation
    /// </summary>
    /// <param name="tipseg">Insurance Type</param>
    /// <param name="orgsin">Claim Origin</param>
    /// <param name="rmosin">Claim Branch</param>
    /// <param name="numsin">Claim Number</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (max 100)</param>
    /// <returns>Paginated history records ordered by occurrence DESC</returns>
    Task<ClaimHistoryResponse> GetClaimHistoryAsync(int tipseg, int orgsin, int rmosin, int numsin, int page = 1, int pageSize = 20);
}
