using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Consortium Contract - Consortium housing insurance contracts (Legacy table: EF_CONTR_SEG_HABIT)
/// Contains consortium-specific contract information used to determine validation routing.
/// </summary>
[Table("EF_CONTR_SEG_HABIT")]
[Index(nameof(NumProposta), Name = "IX_EF_CONTR_SEG_HABIT_Proposal")]
[Index(nameof(CpfCnpjSegurado), Name = "IX_EF_CONTR_SEG_HABIT_TaxId")]
public class ConsortiumContract
{
    /// <summary>
    /// Número do Contrato - Contract Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUM_CONTRATO")]
    [Required]
    public int NumContrato { get; set; }

    /// <summary>
    /// Número da Proposta - Proposal Number
    /// </summary>
    [Column("NUM_PROPOSTA")]
    [Required]
    public int NumProposta { get; set; }

    /// <summary>
    /// CPF/CNPJ do Segurado - Insured Tax ID
    /// </summary>
    [Column("CPF_CNPJ_SEGURADO")]
    [Required]
    [StringLength(20)]
    public string CpfCnpjSegurado { get; set; } = string.Empty;

    /// <summary>
    /// Nome do Segurado - Insured Name
    /// </summary>
    [Column("NOME_SEGURADO")]
    [Required]
    [StringLength(200)]
    public string NomeSegurado { get; set; } = string.Empty;

    /// <summary>
    /// Número da Apólice - Policy Number
    /// </summary>
    [Column("NUM_APOLICE")]
    public int? NumApolice { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Contract Start Date
    /// </summary>
    [Column("DATA_INICIO_VIGENCIA", TypeName = "date")]
    [Required]
    public DateTime DataInicioVigencia { get; set; }

    /// <summary>
    /// Data de Fim de Vigência - Contract End Date
    /// </summary>
    [Column("DATA_FIM_VIGENCIA", TypeName = "date")]
    [Required]
    public DateTime DataFimVigencia { get; set; }

    /// <summary>
    /// Valor Segurado - Insured Amount
    /// </summary>
    [Column("VALOR_SEGURADO", TypeName = "decimal(15,2)")]
    [Range(0, 999999999999.99)]
    public decimal? ValorSegurado { get; set; }

    /// <summary>
    /// Código do Produto - Product Code
    /// </summary>
    [Column("COD_PRODUTO")]
    [Required]
    public int CodProduto { get; set; }

    /// <summary>
    /// Situação do Contrato - Contract Status
    /// A=Active, C=Cancelled, S=Suspended
    /// </summary>
    [Column("SITUACAO_CONTRATO")]
    [Required]
    [StringLength(1)]
    [RegularExpression("^[ACS]$", ErrorMessage = "Status must be 'A' (Active), 'C' (Cancelled), or 'S' (Suspended)")]
    public string SituacaoContrato { get; set; } = "A";

    /// <summary>
    /// Tipo de Contrato - Contract Type
    /// EFP or HB
    /// </summary>
    [Column("TIPO_CONTRATO")]
    [StringLength(10)]
    public string? TipoContrato { get; set; }

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

    /// <summary>
    /// Check if contract is EFP type (routes to SIPUA validation)
    /// </summary>
    [NotMapped]
    public bool IsEfpContract => NumContrato > 0 && TipoContrato == "EFP";

    /// <summary>
    /// Check if contract is HB type (routes to SIMDA validation)
    /// </summary>
    [NotMapped]
    public bool IsHbContract => NumContrato == 0 || TipoContrato == "HB";

    /// <summary>
    /// Check if contract is active
    /// </summary>
    [NotMapped]
    public bool IsActive
    {
        get
        {
            if (SituacaoContrato != "A")
                return false;

            var today = DateTime.Today;
            return today >= DataInicioVigencia && today <= DataFimVigencia;
        }
    }
}
