using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim History - Payment authorization transactions (Legacy table: THISTSIN)
/// Records each payment authorization with amounts, beneficiary, and operator information.
/// </summary>
[Table("THISTSIN")]
[Index(nameof(Dtmovto), nameof(Operacao), Name = "IX_THISTSIN_DateOperation")]
[Index(nameof(Ezeusrid), nameof(Dtmovto), Name = "IX_THISTSIN_UserDate")]
public class ClaimHistory
{
    /// <summary>
    /// Tipo de Seguro - Insurance Type (Primary Key)
    /// </summary>
    [Key]
    [Column("TIPSEG", Order = 0)]
    [Required]
    public int Tipseg { get; set; }

    /// <summary>
    /// Órgão do Sinistro - Claim Origin (Primary Key)
    /// </summary>
    [Key]
    [Column("ORGSIN", Order = 1)]
    [Required]
    public int Orgsin { get; set; }

    /// <summary>
    /// Ramo do Sinistro - Claim Branch (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOSIN", Order = 2)]
    [Required]
    public int Rmosin { get; set; }

    /// <summary>
    /// Número do Sinistro - Claim Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUMSIN", Order = 3)]
    [Required]
    public int Numsin { get; set; }

    /// <summary>
    /// Ocorrência do Histórico - Occurrence Sequence (Primary Key)
    /// </summary>
    [Key]
    [Column("OCORHIST", Order = 4)]
    [Required]
    public int Ocorhist { get; set; }

    /// <summary>
    /// Operação - Operation Code
    /// Always 1098 for payment authorization
    /// </summary>
    [Column("OPERACAO")]
    [Required]
    public int Operacao { get; set; }

    /// <summary>
    /// Data do Movimento - Transaction Date
    /// </summary>
    [Column("DTMOVTO", TypeName = "date")]
    [Required]
    public DateTime Dtmovto { get; set; }

    /// <summary>
    /// Hora da Operação - Transaction Time
    /// Format: HHmmss
    /// </summary>
    [Column("HORAOPER")]
    [Required]
    [StringLength(6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Time must be in HHmmss format")]
    public string Horaoper { get; set; } = string.Empty;

    /// <summary>
    /// Valor Principal - Principal Amount (original currency)
    /// </summary>
    [Column("VALPRI", TypeName = "decimal(15,2)")]
    [Required]
    [Range(0.01, 999999999999.99)]
    public decimal Valpri { get; set; }

    /// <summary>
    /// Correção Monetária - Correction Amount (original currency)
    /// </summary>
    [Column("CRRMON", TypeName = "decimal(15,2)")]
    [Range(0, 999999999999.99)]
    public decimal Crrmon { get; set; }

    /// <summary>
    /// Nome do Favorecido - Beneficiary Name
    /// Mandatory when claim insurance type (tpsegu) != 0
    /// </summary>
    [Column("NOMFAV")]
    [StringLength(100)]
    public string? Nomfav { get; set; }

    /// <summary>
    /// Tipo de Correção - Correction Type
    /// Always '5' for payment authorizations
    /// </summary>
    [Column("TIPCRR")]
    [Required]
    [StringLength(1)]
    public string Tipcrr { get; set; } = "5";

    /// <summary>
    /// Valor Principal em BTNF - Principal in standardized currency
    /// </summary>
    [Column("VALPRIBT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Valpribt { get; set; }

    /// <summary>
    /// Correção em BTNF - Correction in standardized currency
    /// </summary>
    [Column("CRRMONBT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Crrmonbt { get; set; }

    /// <summary>
    /// Valor Total em BTNF - Total Value in BTNF
    /// Sum of VALPRIBT + CRRMONBT
    /// </summary>
    [Column("VALTOTBT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Valtotbt { get; set; }

    /// <summary>
    /// Situação Contábil - Accounting Status
    /// Initialized as '0'
    /// </summary>
    [Column("SITCONTB")]
    [Required]
    [StringLength(1)]
    public string Sitcontb { get; set; } = "0";

    /// <summary>
    /// Situação - Overall Status
    /// Initialized as '0'
    /// </summary>
    [Column("SITUACAO")]
    [Required]
    [StringLength(1)]
    public string Situacao { get; set; } = "0";

    /// <summary>
    /// ID do Usuário - Operator User ID
    /// </summary>
    [Column("EZEUSRID")]
    [Required]
    [StringLength(50)]
    public string Ezeusrid { get; set; } = string.Empty;

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
    /// Parent claim record
    /// </summary>
    [ForeignKey($"{nameof(Tipseg)},{nameof(Orgsin)},{nameof(Rmosin)},{nameof(Numsin)}")]
    public virtual ClaimMaster? Claim { get; set; }

    /// <summary>
    /// Combined transaction timestamp
    /// </summary>
    [NotMapped]
    public DateTime TransactionTimestamp
    {
        get
        {
            if (string.IsNullOrEmpty(Horaoper) || Horaoper.Length != 6)
                return Dtmovto;

            int hour = int.Parse(Horaoper.Substring(0, 2));
            int minute = int.Parse(Horaoper.Substring(2, 2));
            int second = int.Parse(Horaoper.Substring(4, 2));

            return Dtmovto.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
        }
    }
}
