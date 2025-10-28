namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Payload sent to CNOUA, SIPUA, SIMDA validation services
/// </summary>
public class ExternalValidationRequest
{
    /// <summary>
    /// Claim source identifier
    /// </summary>
    public int Fonte { get; set; }

    /// <summary>
    /// Protocol number
    /// </summary>
    public int Protsini { get; set; }

    /// <summary>
    /// DAC verification digit
    /// </summary>
    public int Dac { get; set; }

    /// <summary>
    /// Origin code (2 digits, 01-99)
    /// </summary>
    public int Orgsin { get; set; }

    /// <summary>
    /// Branch code (2 digits, 00-99)
    /// </summary>
    public int Rmosin { get; set; }

    /// <summary>
    /// Claim number (1-6 digits)
    /// </summary>
    public int Numsin { get; set; }

    /// <summary>
    /// Product code (6814, 7701, 7709 for consortium)
    /// </summary>
    public int CodProdu { get; set; }

    /// <summary>
    /// Contract number (nullable, from EF_CONTR_SEG_HABIT lookup)
    /// </summary>
    public int? NumContrato { get; set; }

    /// <summary>
    /// Payment type (1-5)
    /// </summary>
    public int TipoPagamento { get; set; }

    /// <summary>
    /// Principal amount in original currency
    /// </summary>
    public decimal ValorPrincipal { get; set; }

    /// <summary>
    /// Correction/interest amount
    /// </summary>
    public decimal ValorCorrecao { get; set; }

    /// <summary>
    /// Beneficiary name (max 255 chars, nullable)
    /// </summary>
    public string? Beneficiario { get; set; }

    /// <summary>
    /// EZEUSRID operator making request
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;
}
