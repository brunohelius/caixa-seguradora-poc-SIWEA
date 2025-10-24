using AutoMapper;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;

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

        // ClaimHistory to HistoryRecordDto (will be created later)
        CreateMap<ClaimHistory, HistoryRecordDto>()
            .ForMember(dest => dest.DataHoraFormatada, opt => opt.MapFrom(src =>
                $"{src.Dtmovto:dd/MM/yyyy} {src.Horaoper:hh\\:mm\\:ss}"));

        // ClaimPhase to PhaseRecordDto (will be created later)
        CreateMap<ClaimPhase, PhaseRecordDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.IsOpen ? "Aberta" : "Fechada"))
            .ForMember(dest => dest.DiasAberta, opt => opt.MapFrom(src =>
                src.DaysOpen));
    }
}

/// <summary>
/// History Record DTO
/// </summary>
public class HistoryRecordDto
{
    public int Tipseg { get; set; }
    public int Orgsin { get; set; }
    public int Rmosin { get; set; }
    public int Numsin { get; set; }
    public int Ocorhist { get; set; }
    public int Operacao { get; set; }
    public DateTime Dtmovto { get; set; }
    public TimeSpan Horaoper { get; set; }
    public string DataHoraFormatada { get; set; } = string.Empty;
    public decimal Valpri { get; set; }
    public decimal Crrmon { get; set; }
    public string? Nomfav { get; set; }
    public string Tipcrr { get; set; } = string.Empty;
    public decimal Valpribt { get; set; }
    public decimal Crrmonbt { get; set; }
    public decimal Valtotbt { get; set; }
    public string Sitcontb { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public string Ezeusrid { get; set; } = string.Empty;
}

/// <summary>
/// Phase Record DTO
/// </summary>
public class PhaseRecordDto
{
    public int Fonte { get; set; }
    public int Protsini { get; set; }
    public int Dac { get; set; }
    public int CodFase { get; set; }
    public int CodEvento { get; set; }
    public int NumOcorrSiniaco { get; set; }
    public DateTime DataInivigRefaev { get; set; }
    public DateTime DataAberturaSifa { get; set; }
    public DateTime DataFechaSifa { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DiasAberta { get; set; }
}
