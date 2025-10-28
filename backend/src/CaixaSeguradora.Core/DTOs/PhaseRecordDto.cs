namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// T100 [US5] - Phase Record DTO
/// Represents a single claim phase with opening/closing dates and computed properties
/// </summary>
public class PhaseRecordDto
{
    public int CodFase { get; set; }
    public string? NomeFase { get; set; }
    public int CodEvento { get; set; }
    public string? NomeEvento { get; set; }
    public int NumOcorrSiniaco { get; set; }
    public DateTime DataInivigRefaev { get; set; }
    public DateTime DataAberturaSifa { get; set; }
    public DateTime DataFechaSifa { get; set; }

    /// <summary>
    /// Phase is open if DataFechaSifa equals 9999-12-31
    /// </summary>
    public bool IsOpen => DataFechaSifa == new DateTime(9999, 12, 31);

    /// <summary>
    /// Status description in Portuguese
    /// </summary>
    public string Status => IsOpen ? "Aberta" : "Fechada";

    /// <summary>
    /// Days the phase has been open (or was open before closing)
    /// </summary>
    public int DiasAberta
    {
        get
        {
            var endDate = IsOpen ? DateTime.Today : DataFechaSifa;
            return (endDate - DataAberturaSifa.Date).Days;
        }
    }

    /// <summary>
    /// Formatted opening date (dd/MM/yyyy)
    /// </summary>
    public string DataAberturaFormatada => DataAberturaSifa.ToString("dd/MM/yyyy");

    /// <summary>
    /// Formatted closing date (dd/MM/yyyy or "Em Aberto")
    /// </summary>
    public string DataFechaFormatada => IsOpen ? "Em Aberto" : DataFechaSifa.ToString("dd/MM/yyyy");

    /// <summary>
    /// Color indicator for phase duration
    /// green: < 30 days, yellow: 30-60 days, red: > 60 days
    /// </summary>
    public string DurationColorCode
    {
        get
        {
            if (DiasAberta < 30) return "green";
            if (DiasAberta <= 60) return "yellow";
            return "red";
        }
    }
}

/// <summary>
/// T100 [US5] - Phase Response
/// Contains all phases for a claim protocol
/// </summary>
public class PhaseResponse
{
    public bool Sucesso { get; set; } = true;
    public string Protocolo { get; set; } = string.Empty;
    public int TotalFases { get; set; }
    public List<PhaseRecordDto> Fases { get; set; } = new();
}
