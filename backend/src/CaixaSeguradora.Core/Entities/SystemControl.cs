using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// System Control - System-wide configuration (Legacy table: TSISTEMA)
/// Provides current business date for the claims system.
/// </summary>
[Table("TSISTEMA")]
public class SystemControl
{
    /// <summary>
    /// ID do Sistema - System ID (Primary Key)
    /// For claims system: 'SI'
    /// </summary>
    [Key]
    [Column("IDSISTEM")]
    [Required]
    [StringLength(10)]
    public string Idsistem { get; set; } = string.Empty;

    /// <summary>
    /// Data de Movimento em Aberto - Current Business Date
    /// Used to timestamp all transactions
    /// </summary>
    [Column("DTMOVABE", TypeName = "date")]
    [Required]
    public DateTime Dtmovabe { get; set; }

    /// <summary>
    /// Nome do Sistema - System Name
    /// </summary>
    [Column("NOMESIST")]
    [StringLength(100)]
    public string? Nomesist { get; set; }

    /// <summary>
    /// Sistema Ativo - System Active Status
    /// </summary>
    [Column("ATIVO")]
    [Required]
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data de Fechamento - Last Closing Date
    /// </summary>
    [Column("DTFECHA", TypeName = "date")]
    public DateTime? Dtfecha { get; set; }

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
}
