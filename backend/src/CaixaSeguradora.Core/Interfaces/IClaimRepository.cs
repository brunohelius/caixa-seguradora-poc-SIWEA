using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Specialized repository interface for ClaimMaster entity
/// Provides claim-specific query methods in addition to base repository operations
/// </summary>
public interface IClaimRepository : IRepository<ClaimMaster>
{
    /// <summary>
    /// Searches for a claim by protocol information
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Check digit (0-9)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ClaimMaster if found, null otherwise</returns>
    Task<ClaimMaster?> SearchByProtocolAsync(
        int fonte,
        int protsini,
        int dac,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for a claim by claim number
    /// </summary>
    /// <param name="orgsin">Claim origin</param>
    /// <param name="rmosin">Claim branch</param>
    /// <param name="numsin">Claim number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ClaimMaster if found, null otherwise</returns>
    Task<ClaimMaster?> SearchByClaimNumberAsync(
        int orgsin,
        int rmosin,
        int numsin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for claims by leader code
    /// Used for reinsurance claim lookups
    /// </summary>
    /// <param name="codlider">Leader code</param>
    /// <param name="sinlid">Leader claim number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching claims</returns>
    Task<IEnumerable<ClaimMaster>> SearchByLeaderCodeAsync(
        int codlider,
        int sinlid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated claim history for a specific claim
    /// </summary>
    /// <param name="tipseg">Insurance type</param>
    /// <param name="orgsin">Claim origin</param>
    /// <param name="rmosin">Claim branch</param>
    /// <param name="numsin">Claim number</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result with history records</returns>
    Task<PagedResult<ClaimHistory>> GetClaimHistoryAsync(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets claim phases for a specific protocol
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Check digit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of claim phases</returns>
    Task<IEnumerable<ClaimPhase>> GetClaimPhasesAsync(
        int fonte,
        int protsini,
        int dac,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets claim accompaniment records for a specific protocol
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Check digit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of accompaniment records</returns>
    Task<IEnumerable<ClaimAccompaniment>> GetClaimAccompanimentsAsync(
        int fonte,
        int protsini,
        int dac,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the history occurrence counter for a claim
    /// This is a critical operation used before inserting new history records
    /// </summary>
    /// <param name="tipseg">Insurance type</param>
    /// <param name="orgsin">Claim origin</param>
    /// <param name="rmosin">Claim branch</param>
    /// <param name="numsin">Claim number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New occurrence number</returns>
    Task<int> IncrementOcorhistAsync(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for claims by policy number
    /// </summary>
    /// <param name="orgapo">Policy origin</param>
    /// <param name="rmoapo">Policy branch</param>
    /// <param name="numapol">Policy number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of claims for the policy</returns>
    Task<IEnumerable<ClaimMaster>> SearchByPolicyAsync(
        int orgapo,
        int rmoapo,
        int numapol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets claims filtered by product code
    /// Useful for consortium product filtering (6814, 7701, 7709)
    /// </summary>
    /// <param name="codprodu">Product code</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated result with claims</returns>
    Task<PagedResult<ClaimMaster>> GetClaimsByProductAsync(
        int codprodu,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a paginated result set
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
