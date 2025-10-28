using System;

namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Claim Detail DTO
/// Contains all information about a claim for display purposes
/// </summary>
public class ClaimDetailDto
{
    // Primary Key
    public int Tipseg { get; set; }
    public int Orgsin { get; set; }
    public int Rmosin { get; set; }
    public int Numsin { get; set; }

    // Protocol Information
    public int Fonte { get; set; }
    public int Protsini { get; set; }
    public int Dac { get; set; }

    /// <summary>
    /// Formatted Protocol Number: "fonte/protsini-dac" (e.g., "001/0123456-7")
    /// </summary>
    public string NumeroProtocolo { get; set; } = string.Empty;

    /// <summary>
    /// Formatted Claim Number: "orgsin/rmosin/numsin" (e.g., "001/001/0000123")
    /// </summary>
    public string NumeroSinistro { get; set; } = string.Empty;

    // Policy Information
    public int Orgapo { get; set; }
    public int Rmoapo { get; set; }
    public int Numapol { get; set; }

    /// <summary>
    /// Formatted Policy Number: "orgapo/rmoapo/numapol"
    /// </summary>
    public string NumeroApolice { get; set; } = string.Empty;

    /// <summary>
    /// Nome do Segurado - Insured Name (from Policy)
    /// </summary>
    public string? NomeSeguradora { get; set; }

    /// <summary>
    /// Nome do Ramo - Branch Name
    /// </summary>
    public string? NomeRamo { get; set; }

    // Product Information
    public int Codprodu { get; set; }

    /// <summary>
    /// Indica se é produto de consórcio (codes: 6814, 7701, 7709)
    /// </summary>
    public bool EhConsorcio { get; set; }

    // Financial Information
    /// <summary>
    /// Saldo a Pagar - Expected Reserve Amount
    /// </summary>
    public decimal Sdopag { get; set; }

    /// <summary>
    /// Total Pago - Total Payments Made
    /// </summary>
    public decimal Totpag { get; set; }

    /// <summary>
    /// Valor Pendente - Pending Value (Sdopag - Totpag)
    /// </summary>
    public decimal ValorPendente { get; set; }

    // Leader Information (for reinsurance)
    public int? Codlider { get; set; }
    public int? Sinlid { get; set; }

    // Workflow Information
    /// <summary>
    /// Ocorrência do Histórico - History Occurrence Counter
    /// </summary>
    public int Ocorhist { get; set; }

    /// <summary>
    /// Tipo de Registro - Policy Type (1 or 2)
    /// </summary>
    public string Tipreg { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de Seguro - Insurance Type (0 = optional beneficiary, != 0 = mandatory)
    /// </summary>
    public int Tpsegu { get; set; }

    // Audit Fields
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
