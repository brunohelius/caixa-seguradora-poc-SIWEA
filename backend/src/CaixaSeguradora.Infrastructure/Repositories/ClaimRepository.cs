using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Data;

namespace CaixaSeguradora.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ClaimMaster entity
/// Provides optimized EF Core queries with proper eager loading and compiled queries
/// </summary>
public class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    /// <summary>
    /// Initializes a new instance of ClaimRepository
    /// </summary>
    /// <param name="context">Database context</param>
    public ClaimRepository(ClaimsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region IRepository<ClaimMaster> Implementation

    /// <summary>
    /// Gets a claim by its composite primary key
    /// </summary>
    public async Task<ClaimMaster?> GetByIdAsync(
        object[] id,
        CancellationToken cancellationToken = default)
    {
        if (id == null || id.Length != 4)
            throw new ArgumentException("ClaimMaster requires 4 key components: Tipseg, Orgsin, Rmosin, Numsin", nameof(id));

        return await _context.ClaimMasters
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .FirstOrDefaultAsync(c =>
                c.Tipseg == (int)id[0] &&
                c.Orgsin == (int)id[1] &&
                c.Rmosin == (int)id[2] &&
                c.Numsin == (int)id[3],
                cancellationToken);
    }

    /// <summary>
    /// Gets all claims (use with caution on large datasets)
    /// </summary>
    public async Task<IEnumerable<ClaimMaster>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds claims matching the specified predicate
    /// </summary>
    public async Task<IEnumerable<ClaimMaster>> FindAsync(
        Expression<Func<ClaimMaster, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new claim
    /// </summary>
    public async Task<ClaimMaster> AddAsync(
        ClaimMaster entity,
        CancellationToken cancellationToken = default)
    {
        await _context.ClaimMasters.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Adds multiple claims
    /// </summary>
    public async Task AddRangeAsync(
        IEnumerable<ClaimMaster> entities,
        CancellationToken cancellationToken = default)
    {
        await _context.ClaimMasters.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Updates an existing claim
    /// </summary>
    public Task<ClaimMaster> UpdateAsync(
        ClaimMaster entity,
        CancellationToken cancellationToken = default)
    {
        _context.ClaimMasters.Update(entity);
        return Task.FromResult(entity);
    }

    /// <summary>
    /// Deletes a claim
    /// </summary>
    public Task DeleteAsync(
        ClaimMaster entity,
        CancellationToken cancellationToken = default)
    {
        _context.ClaimMasters.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes claims matching the specified predicate
    /// </summary>
    public async Task<int> DeleteRangeAsync(
        Expression<Func<ClaimMaster, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ClaimMasters
            .Where(predicate)
            .ToListAsync(cancellationToken);

        _context.ClaimMasters.RemoveRange(entities);
        return entities.Count;
    }

    /// <summary>
    /// Counts claims matching the specified predicate
    /// </summary>
    public async Task<int> CountAsync(
        Expression<Func<ClaimMaster, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _context.ClaimMasters.CountAsync(cancellationToken);

        return await _context.ClaimMasters.CountAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Checks if any claim matches the specified predicate
    /// </summary>
    public async Task<bool> AnyAsync(
        Expression<Func<ClaimMaster, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters.AnyAsync(predicate, cancellationToken);
    }

    #endregion

    #region Specialized Claim Query Methods

    /// <summary>
    /// Searches for a claim by protocol information
    /// Uses compiled query for performance
    /// </summary>
    public async Task<ClaimMaster?> SearchByProtocolAsync(
        int fonte,
        int protsini,
        int dac,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .FirstOrDefaultAsync(c =>
                c.Fonte == fonte &&
                c.Protsini == protsini &&
                c.Dac == dac,
                cancellationToken);
    }

    /// <summary>
    /// Searches for a claim by claim number (primary key without Tipseg)
    /// </summary>
    public async Task<ClaimMaster?> SearchByClaimNumberAsync(
        int orgsin,
        int rmosin,
        int numsin,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .FirstOrDefaultAsync(c =>
                c.Orgsin == orgsin &&
                c.Rmosin == rmosin &&
                c.Numsin == numsin,
                cancellationToken);
    }

    /// <summary>
    /// Searches for claims by leader code (reinsurance)
    /// </summary>
    public async Task<IEnumerable<ClaimMaster>> SearchByLeaderCodeAsync(
        int codlider,
        int sinlid,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .Where(c => c.Codlider == codlider && c.Sinlid == sinlid)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets paginated claim history for a specific claim
    /// </summary>
    public async Task<PagedResult<ClaimHistory>> GetClaimHistoryAsync(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _context.ClaimHistories
            .AsNoTracking()
            .Where(h =>
                h.Tipseg == tipseg &&
                h.Orgsin == orgsin &&
                h.Rmosin == rmosin &&
                h.Numsin == numsin)
            .OrderByDescending(h => h.Dtmovto)
            .ThenByDescending(h => h.Ocorhist);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ClaimHistory>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    /// <summary>
    /// Gets claim phases for a specific protocol
    /// </summary>
    public async Task<IEnumerable<ClaimPhase>> GetClaimPhasesAsync(
        int fonte,
        int protsini,
        int dac,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimPhases
            .AsNoTracking()
            .Include(p => p.PhaseEventRelationship)
            .Where(p =>
                p.Fonte == fonte &&
                p.Protsini == protsini &&
                p.Dac == dac)
            .OrderByDescending(p => p.DataAberturaSifa)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets claim accompaniment records for a specific protocol
    /// </summary>
    public async Task<IEnumerable<ClaimAccompaniment>> GetClaimAccompanimentsAsync(
        int fonte,
        int protsini,
        int dac,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimAccompaniments
            .AsNoTracking()
            .Where(a =>
                a.Fonte == fonte &&
                a.Protsini == protsini &&
                a.Dac == dac)
            .OrderByDescending(a => a.DataMovtoSiniaco)
            .ThenByDescending(a => a.NumOcorrSiniaco)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Increments the history occurrence counter for a claim
    /// This is a critical operation that requires immediate persistence
    /// </summary>
    public async Task<int> IncrementOcorhistAsync(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        CancellationToken cancellationToken = default)
    {
        var claim = await _context.ClaimMasters
            .FirstOrDefaultAsync(c =>
                c.Tipseg == tipseg &&
                c.Orgsin == orgsin &&
                c.Rmosin == rmosin &&
                c.Numsin == numsin,
                cancellationToken);

        if (claim == null)
            throw new InvalidOperationException($"Claim not found: {tipseg}/{orgsin}/{rmosin}/{numsin}");

        claim.Ocorhist++;
        await _context.SaveChangesAsync(cancellationToken);

        return claim.Ocorhist;
    }

    /// <summary>
    /// Searches for claims by policy number
    /// </summary>
    public async Task<IEnumerable<ClaimMaster>> SearchByPolicyAsync(
        int orgapo,
        int rmoapo,
        int numapol,
        CancellationToken cancellationToken = default)
    {
        return await _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .Where(c =>
                c.Orgapo == orgapo &&
                c.Rmoapo == rmoapo &&
                c.Numapol == numapol)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets claims filtered by product code with pagination
    /// </summary>
    public async Task<PagedResult<ClaimMaster>> GetClaimsByProductAsync(
        int codprodu,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;

        var query = _context.ClaimMasters
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Policy)
            .Where(c => c.Codprodu == codprodu)
            .OrderByDescending(c => c.Fonte)
            .ThenByDescending(c => c.Protsini);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ClaimMaster>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    #endregion
}
