using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Policy Master - Policy and insured information (Legacy table: TAPOLICE)
/// Contains policy details and insured party name.
/// </summary>
[Table("TAPOLICE")]
[Index(nameof(Nome), Name = "IX_TAPOLICE_InsuredName")]
public class PolicyMaster
{
    /// <summary>
    /// Órgão da Apólice - Policy Origin (Primary Key)
    /// </summary>
    [Key]
    [Column("ORGAPO", Order = 0)]
    [Required]
    public int Orgapo { get; set; }

    /// <summary>
    /// Ramo da Apólice - Policy Branch (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOAPO", Order = 1)]
    [Required]
    public int Rmoapo { get; set; }

    /// <summary>
    /// Número da Apólice - Policy Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUMAPOL", Order = 2)]
    [Required]
    public int Numapol { get; set; }

    /// <summary>
    /// Nome do Segurado - Insured Name
    /// </summary>
    [Column("NOME")]
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF/CNPJ do Segurado - Insured Tax ID
    /// </summary>
    [Column("CPFCNPJ")]
    [StringLength(20)]
    public string? Cpfcnpj { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Policy Start Date
    /// </summary>
    [Column("DTINIVIG", TypeName = "date")]
    public DateTime? Dtinivig { get; set; }

    /// <summary>
    /// Data de Fim de Vigência - Policy End Date
    /// </summary>
    [Column("DTFIMVIG", TypeName = "date")]
    public DateTime? Dtfimvig { get; set; }

    /// <summary>
    /// Situação da Apólice - Policy Status
    /// </summary>
    [Column("SITUACAO")]
    [StringLength(1)]
    public string? Situacao { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Branch information
    /// </summary>
    [ForeignKey(nameof(Rmoapo))]
    public virtual BranchMaster? Branch { get; set; }

    /// <summary>
    /// Check if policy is active
    /// </summary>
    [NotMapped]
    public bool IsActive
    {
        get
        {
            if (!Dtinivig.HasValue || !Dtfimvig.HasValue)
                return false;

            var today = DateTime.Today;
            return today >= Dtinivig.Value && today <= Dtfimvig.Value;
        }
    }
}
