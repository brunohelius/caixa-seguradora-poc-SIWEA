using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Currency Unit Table - Conversion rates (Legacy table: TGEUNIMO)
/// Provides currency conversion rates to BTNF (Bônus do Tesouro Nacional Fiscal) with validity periods.
/// </summary>
[Table("TGEUNIMO")]
[Index(nameof(Dtinivig), nameof(Dttervig), Name = "IX_TGEUNIMO_ValidityPeriod")]
public class CurrencyUnit
{
    /// <summary>
    /// Data de Início de Vigência - Start Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DTINIVIG", TypeName = "date", Order = 0)]
    [Required]
    public DateTime Dtinivig { get; set; }

    /// <summary>
    /// Data de Término de Vigência - End Date
    /// </summary>
    [Column("DTTERVIG", TypeName = "date")]
    [Required]
    public DateTime Dttervig { get; set; }

    /// <summary>
    /// Valor em Cruzados - Conversion Rate to BTNF
    /// Used to convert claim amounts to standardized currency
    /// </summary>
    [Column("VLCRUZAD", TypeName = "decimal(18,6)")]
    [Required]
    [Range(0.000001, 999999999999.999999)]
    public decimal Vlcruzad { get; set; }

    /// <summary>
    /// Código da Moeda - Currency Code
    /// </summary>
    [Column("CODMOEDA")]
    [StringLength(10)]
    public string? Codmoeda { get; set; }

    /// <summary>
    /// Nome da Moeda - Currency Name
    /// </summary>
    [Column("NOMEMOEDA")]
    [StringLength(50)]
    public string? Nomemoeda { get; set; }

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
    /// Check if date is within validity period
    /// </summary>
    public bool IsValidForDate(DateTime date)
    {
        return date >= Dtinivig && date <= Dttervig;
    }
}
