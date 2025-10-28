namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// T077 [US3] - History Record DTO
/// Represents a single payment history record with formatted dates
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

    /// <summary>
    /// Formatted date and time (dd/MM/yyyy HH:mm:ss)
    /// </summary>
    public string DataHoraFormatada => $"{Dtmovto:dd/MM/yyyy} {Horaoper:hh\\:mm\\:ss}";

    /// <summary>
    /// Combined DateTime for sorting
    /// </summary>
    public DateTime DataHoraCompleta => Dtmovto.Add(Horaoper);

    public decimal Valpri { get; set; }
    public decimal Crrmon { get; set; }
    public string? Nomfav { get; set; }
    public string? Tipcrr { get; set; }
    public decimal Valpribt { get; set; }
    public decimal Crrmonbt { get; set; }
    public decimal Valtotbt { get; set; }
    public string? Sitcontb { get; set; }
    public string? Situacao { get; set; }
    public string? Ezeusrid { get; set; }
}

/// <summary>
/// T078 [US3] - Claim History Response
/// Paginated response for claim history
/// </summary>
public class ClaimHistoryResponse
{
    public bool Sucesso { get; set; } = true;
    public int TotalRegistros { get; set; }
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas { get; set; }
    public List<HistoryRecordDto> Historico { get; set; } = new();
}
