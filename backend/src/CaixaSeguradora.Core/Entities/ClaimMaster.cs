using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim Master - Main claim record (Legacy table: TMESTSIN)
/// Represents the primary claim entity with protocol identification, policy references, and financial summary.
/// </summary>
[Table("TMESTSIN")]
public class ClaimMaster
{
    /// <summary>
    /// Tipo de Seguro - Insurance Type (Primary Key Component 1)
    /// </summary>
    [Required]
    public int Tipseg { get; set; }

    /// <summary>
    /// Órgão do Sinistro - Claim Origin (Primary Key Component 2)
    /// </summary>
    [Required]
    public int Orgsin { get; set; }

    /// <summary>
    /// Ramo do Sinistro - Claim Branch (Primary Key Component 3)
    /// </summary>
    [Required]
    public int Rmosin { get; set; }

    /// <summary>
    /// Número do Sinistro - Claim Number (Primary Key Component 4)
    /// </summary>
    [Required]
    public int Numsin { get; set; }

    /// <summary>
    /// Fonte do Protocolo - Protocol Source
    /// </summary>
    [Required]
    public int Fonte { get; set; }

    /// <summary>
    /// Número do Protocolo - Protocol Number
    /// </summary>
    [Required]
    public int Protsini { get; set; }

    /// <summary>
    /// Dígito de Controle - Check Digit (0-9)
    /// </summary>
    [Required]
    [Range(0, 9)]
    public int Dac { get; set; }

    /// <summary>
    /// Órgão da Apólice - Policy Origin
    /// </summary>
    [Required]
    public int Orgapo { get; set; }

    /// <summary>
    /// Ramo da Apólice - Policy Branch
    /// </summary>
    [Required]
    public int Rmoapo { get; set; }

    /// <summary>
    /// Número da Apólice - Policy Number
    /// </summary>
    [Required]
    public int Numapol { get; set; }

    /// <summary>
    /// Código do Produto - Product Code
    /// </summary>
    [Required]
    public int Codprodu { get; set; }

    /// <summary>
    /// Saldo a Pagar - Expected Reserve Amount (15,2 precision)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(15,2)")]
    [Range(0, 999999999999.99)]
    public decimal Sdopag { get; set; }

    /// <summary>
    /// Total Pago - Total Payments Made (15,2 precision)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(15,2)")]
    [Range(0, 999999999999.99)]
    public decimal Totpag { get; set; }

    /// <summary>
    /// Código do Líder - Leader Code (for reinsurance) - Nullable
    /// </summary>
    public int? Codlider { get; set; }

    /// <summary>
    /// Sinistro do Líder - Leader Claim Number - Nullable
    /// </summary>
    public int? Sinlid { get; set; }

    /// <summary>
    /// Ocorrência do Histórico - History Occurrence Counter
    /// Used to track the number of history records
    /// </summary>
    [Required]
    public int Ocorhist { get; set; }

    /// <summary>
    /// Tipo de Registro - Policy Type Indicator (Values: '1' or '2')
    /// </summary>
    [Required]
    [StringLength(1)]
    [RegularExpression("^[12]$", ErrorMessage = "Policy Type must be '1' or '2'")]
    public string Tipreg { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de Seguro (from policy) - Insurance Type from Policy
    /// Used for beneficiary validation: 0 = optional beneficiary, != 0 = mandatory beneficiary
    /// </summary>
    [Required]
    public int Tpsegu { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Branch information
    /// </summary>
    public virtual BranchMaster? Branch { get; set; }

    /// <summary>
    /// Policy information
    /// </summary>
    public virtual PolicyMaster? Policy { get; set; }

    /// <summary>
    /// Claim history records
    /// </summary>
    public virtual ICollection<ClaimHistory> ClaimHistories { get; set; } = new List<ClaimHistory>();

    /// <summary>
    /// Claim accompaniment records
    /// </summary>
    public virtual ICollection<ClaimAccompaniment> ClaimAccompaniments { get; set; } = new List<ClaimAccompaniment>();

    /// <summary>
    /// Claim phase records
    /// </summary>
    public virtual ICollection<ClaimPhase> ClaimPhases { get; set; } = new List<ClaimPhase>();

    // Computed Properties
    /// <summary>
    /// Calculated pending value (Sdopag - Totpag)
    /// </summary>
    [NotMapped]
    public decimal PendingValue => Sdopag - Totpag;

    /// <summary>
    /// Check if this is a consortium product (codes: 6814, 7701, 7709)
    /// </summary>
    [NotMapped]
    public bool IsConsortiumProduct => Codprodu == 6814 || Codprodu == 7701 || Codprodu == 7709;
}
