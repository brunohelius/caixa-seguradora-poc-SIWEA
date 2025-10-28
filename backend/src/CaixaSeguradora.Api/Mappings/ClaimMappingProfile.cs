using AutoMapper;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;
using System;

namespace CaixaSeguradora.Api.Mappings;

/// <summary>
/// AutoMapper Profile for Claim entities and DTOs
/// </summary>
public class ClaimMappingProfile : Profile
{
    public ClaimMappingProfile()
    {
        // ClaimMaster to ClaimDetailDto
        CreateMap<ClaimMaster, ClaimDetailDto>()
            .ForMember(dest => dest.NumeroProtocolo, opt => opt.MapFrom(src =>
                $"{src.Fonte:D3}/{src.Protsini:D7}-{src.Dac}"))
            .ForMember(dest => dest.NumeroSinistro, opt => opt.MapFrom(src =>
                $"{src.Orgsin:D3}/{src.Rmosin:D3}/{src.Numsin:D7}"))
            .ForMember(dest => dest.NumeroApolice, opt => opt.MapFrom(src =>
                $"{src.Orgapo:D3}/{src.Rmoapo:D3}/{src.Numapol:D7}"))
            .ForMember(dest => dest.NomeRamo, opt => opt.MapFrom(src =>
                src.Branch != null ? src.Branch.Nomeramo : null))
            .ForMember(dest => dest.NomeSeguradora, opt => opt.MapFrom(src =>
                src.Policy != null ? src.Policy.Nome : null))
            .ForMember(dest => dest.ValorPendente, opt => opt.MapFrom(src =>
                src.PendingValue))
            .ForMember(dest => dest.EhConsorcio, opt => opt.MapFrom(src =>
                src.IsConsortiumProduct));

        // ClaimHistory to HistoryRecordDto
        // T077 [US3] - Map ClaimHistory entity to HistoryRecordDto with proper time conversion
        CreateMap<ClaimHistory, HistoryRecordDto>()
            .ForMember(dest => dest.Horaoper, opt => opt.MapFrom(src =>
                ParseTimeString(src.Horaoper)))
            .ForMember(dest => dest.DataHoraFormatada, opt => opt.Ignore())
            .ForMember(dest => dest.DataHoraCompleta, opt => opt.Ignore());

        // ClaimPhase to PhaseRecordDto (will be created later)
        CreateMap<ClaimPhase, PhaseRecordDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.IsOpen ? "Aberta" : "Fechada"))
            .ForMember(dest => dest.DiasAberta, opt => opt.MapFrom(src =>
                src.DaysOpen));
    }

    /// <summary>
    /// Parse time string in HHmmss format to TimeSpan
    /// </summary>
    private static TimeSpan ParseTimeString(string horaoper)
    {
        if (string.IsNullOrEmpty(horaoper) || horaoper.Length != 6)
            return TimeSpan.Zero;

        if (!int.TryParse(horaoper.Substring(0, 2), out int hours))
            hours = 0;
        if (!int.TryParse(horaoper.Substring(2, 2), out int minutes))
            minutes = 0;
        if (!int.TryParse(horaoper.Substring(4, 2), out int seconds))
            seconds = 0;

        return new TimeSpan(hours, minutes, seconds);
    }
}

