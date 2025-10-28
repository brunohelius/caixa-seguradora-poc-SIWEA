using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Branch Master - Branch descriptive information (Legacy table: TGERAMO)
/// Contains branch names and organizational information.
/// </summary>
[Table("TGERAMO")]
public class BranchMaster
{
    /// <summary>
    /// Código do Ramo - Branch Code (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOSIN")]
    [Required]
    public int Rmosin { get; set; }

    /// <summary>
    /// Nome do Ramo - Branch Name
    /// </summary>
    [Column("NOMERAMO")]
    [Required]
    [StringLength(100)]
    public string Nomeramo { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do Ramo - Branch Description
    /// </summary>
    [Column("DESCRAMO")]
    [StringLength(500)]
    public string? Descramo { get; set; }

    /// <summary>
    /// Ativo - Active Status
    /// </summary>
    [Column("ATIVO")]
    [Required]
    public bool Ativo { get; set; } = true;

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
