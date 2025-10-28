namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Request to open or close claim phases based on event
/// Validation Rules:
/// - Required: All fields
/// - IndicadorAlteracao ∈ {'1', '2'}
/// - If IndicadorAlteracao = '1', DataFechaeSifa must equal DateTime.Parse("9999-12-31") (open phase marker)
/// - If IndicadorAlteracao = '2', DataFechaeSifa must equal current DTMOVABE (closing today)
/// - CodEvento must equal 1098 (payment authorization event)
///
/// Phase States:
/// Open Phase:   DATA_FECHA_SIFA = '9999-12-31'
/// Closed Phase: DATA_FECHA_SIFA < '9999-12-31' (actual closing date)
///
/// Example for Event 1098 (from SI_REL_FASE_EVENTO):
/// - Phase 10 (Payment Processing):  IND_ALTERACAO_FASE = '1' → OPENS
/// - Phase 5 (Pending Documentation): IND_ALTERACAO_FASE = '2' → CLOSES
/// </summary>
public class PhaseUpdateRequest
{
    /// <summary>
    /// Open phase marker constant (9999-12-31)
    /// </summary>
    public static readonly DateTime OpenPhaseMarker = new DateTime(9999, 12, 31);

    /// <summary>
    /// Claim source
    /// </summary>
    public int Fonte { get; set; }

    /// <summary>
    /// Protocol number
    /// </summary>
    public int Protsini { get; set; }

    /// <summary>
    /// DAC digit
    /// </summary>
    public int Dac { get; set; }

    /// <summary>
    /// Event code (1098 for payment authorization)
    /// </summary>
    public int CodEvento { get; set; }

    /// <summary>
    /// Phase code to open or close
    /// </summary>
    public int CodFase { get; set; }

    /// <summary>
    /// '1' = open phase, '2' = close phase
    /// </summary>
    public string IndicadorAlteracao { get; set; } = string.Empty;

    /// <summary>
    /// Phase effective start date
    /// </summary>
    public DateTime DataInivig { get; set; }

    /// <summary>
    /// Phase closing date ('9999-12-31' if open)
    /// </summary>
    public DateTime DataFechaeSifa { get; set; }

    /// <summary>
    /// Occurrence sequence within phase
    /// </summary>
    public int NumOcorrencia { get; set; }

    /// <summary>
    /// EZEUSRID operator
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// Determines if this request is to open a phase
    /// </summary>
    public bool IsOpenPhaseRequest => IndicadorAlteracao == "1";

    /// <summary>
    /// Determines if this request is to close a phase
    /// </summary>
    public bool IsClosePhaseRequest => IndicadorAlteracao == "2";

    /// <summary>
    /// Validates phase state logic
    /// </summary>
    public bool IsValid()
    {
        // Must be '1' or '2'
        if (IndicadorAlteracao != "1" && IndicadorAlteracao != "2")
            return false;

        // Event code must be 1098
        if (CodEvento != 1098)
            return false;

        // If opening phase, closing date must be 9999-12-31
        if (IsOpenPhaseRequest && DataFechaeSifa != OpenPhaseMarker)
            return false;

        // If closing phase, closing date must be today (DataInivig)
        if (IsClosePhaseRequest && DataFechaeSifa.Date != DataInivig.Date)
            return false;

        return true;
    }
}
