using AutoMapper;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Exceptions;
using CaixaSeguradora.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CaixaSeguradora.Core.Services;

/// <summary>
/// Claim Service Implementation
/// Handles business logic for claim operations
/// </summary>
public class ClaimService : IClaimService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ClaimService> _logger;

    public ClaimService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ClaimService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ClaimDetailDto> SearchClaimAsync(ClaimSearchCriteria criteria)
    {
        if (criteria == null)
            throw new ArgumentNullException(nameof(criteria));

        if (!criteria.IsValid())
            throw new ArgumentException("Critérios de busca inválidos", nameof(criteria));

        ClaimMaster? claim = null;
        string searchType = criteria.GetSearchType();

        _logger.LogInformation("Searching claim using {SearchType}", searchType);

        try
        {
            claim = searchType switch
            {
                "Protocol" => await _unitOfWork.Claims.SearchByProtocolAsync(
                    criteria.Fonte!.Value,
                    criteria.Protsini!.Value,
                    criteria.Dac!.Value),

                "ClaimNumber" => await _unitOfWork.Claims.SearchByClaimNumberAsync(
                    criteria.Orgsin!.Value,
                    criteria.Rmosin!.Value,
                    criteria.Numsin!.Value),

                "LeaderCode" => (await _unitOfWork.Claims.SearchByLeaderCodeAsync(
                    criteria.Codlider!.Value,
                    criteria.Sinlid!.Value)).FirstOrDefault(),

                _ => throw new ArgumentException("Nenhum critério de busca válido fornecido")
            };

            if (claim == null)
            {
                _logger.LogWarning("Claim not found using {SearchType}", searchType);

                throw searchType switch
                {
                    "Protocol" => ClaimNotFoundException.ForProtocol(
                        criteria.Fonte!.Value,
                        criteria.Protsini!.Value,
                        criteria.Dac!.Value),

                    "ClaimNumber" => ClaimNotFoundException.ForClaimNumber(
                        criteria.Orgsin!.Value,
                        criteria.Rmosin!.Value,
                        criteria.Numsin!.Value),

                    "LeaderCode" => ClaimNotFoundException.ForLeaderCode(
                        criteria.Codlider!.Value,
                        criteria.Sinlid!.Value),

                    _ => new ClaimNotFoundException("Sinistro não encontrado")
                };
            }

            var result = _mapper.Map<ClaimDetailDto>(claim);

            _logger.LogInformation("Claim found: Protocol {Protocol}, Claim {ClaimNumber}",
                result.NumeroProtocolo, result.NumeroSinistro);

            return result;
        }
        catch (Exception ex) when (ex is not ClaimNotFoundException)
        {
            _logger.LogError(ex, "Error searching claim using {SearchType}", searchType);
            throw;
        }
    }

    public async Task<ClaimDetailDto> GetClaimByIdAsync(int tipseg, int orgsin, int rmosin, int numsin)
    {
        _logger.LogInformation("Getting claim by ID: {Tipseg}/{Orgsin}/{Rmosin}/{Numsin}",
            tipseg, orgsin, rmosin, numsin);

        var criteria = new ClaimSearchCriteria
        {
            Orgsin = orgsin,
            Rmosin = rmosin,
            Numsin = numsin
        };

        return await SearchClaimAsync(criteria);
    }

    public async Task<bool> ValidateClaimExistsAsync(ClaimSearchCriteria criteria)
    {
        if (criteria == null || !criteria.IsValid())
            return false;

        try
        {
            await SearchClaimAsync(criteria);
            return true;
        }
        catch (ClaimNotFoundException)
        {
            return false;
        }
    }

    public async Task<decimal> GetPendingValueAsync(int tipseg, int orgsin, int rmosin, int numsin)
    {
        var claim = await GetClaimByIdAsync(tipseg, orgsin, rmosin, numsin);
        return claim.ValorPendente;
    }

    /// <summary>
    /// T078 [US3] - Get paginated claim history
    /// </summary>
    public async Task<ClaimHistoryResponse> GetClaimHistoryAsync(
        int tipseg, int orgsin, int rmosin, int numsin, int page = 1, int pageSize = 20)
    {
        _logger.LogInformation(
            "Getting claim history: {Tipseg}/{Orgsin}/{Rmosin}/{Numsin}, Page: {Page}, PageSize: {PageSize}",
            tipseg, orgsin, rmosin, numsin, page, pageSize);

        try
        {
            // Validate claim exists first
            var claimCriteria = new ClaimSearchCriteria
            {
                Orgsin = orgsin,
                Rmosin = rmosin,
                Numsin = numsin
            };

            var claimExists = await ValidateClaimExistsAsync(claimCriteria);
            if (!claimExists)
            {
                throw ClaimNotFoundException.ForClaimNumber(orgsin, rmosin, numsin);
            }

            // Get history from repository
            var allHistory = await _unitOfWork.ClaimHistories.FindAsync(h =>
                h.Tipseg == tipseg &&
                h.Orgsin == orgsin &&
                h.Rmosin == rmosin &&
                h.Numsin == numsin);

            // Sort and paginate
            var orderedHistory = allHistory
                .OrderByDescending(h => h.Ocorhist)
                .ToList();

            var totalRecords = orderedHistory.Count;
            var history = orderedHistory
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Map to DTOs
            var historyDtos = _mapper.Map<List<HistoryRecordDto>>(history);

            var response = new ClaimHistoryResponse
            {
                Sucesso = true,
                TotalRegistros = totalRecords,
                PaginaAtual = page,
                TamanhoPagina = pageSize,
                TotalPaginas = totalPages,
                Historico = historyDtos
            };

            _logger.LogInformation(
                "Retrieved {Count} history records (Page {Page} of {TotalPages})",
                historyDtos.Count, page, totalPages);

            return response;
        }
        catch (Exception ex) when (ex is not ClaimNotFoundException)
        {
            _logger.LogError(ex, "Error retrieving claim history for {Tipseg}/{Orgsin}/{Rmosin}/{Numsin}",
                tipseg, orgsin, rmosin, numsin);
            throw;
        }
    }
}
