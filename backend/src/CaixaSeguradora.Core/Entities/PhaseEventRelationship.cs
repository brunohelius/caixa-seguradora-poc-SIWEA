using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Phase-Event Relationship - Configuration for phase changes (Legacy table: SI_REL_FASE_EVENTO)
/// Defines which phases are affected by each event type and whether they open or close.
/// </summary>
[Table("SI_REL_FASE_EVENTO")]
[Index(nameof(CodEvento), nameof(DataInivigRefaev), Name = "IX_SI_REL_FASE_EVENTO_EventDate")]
public class PhaseEventRelationship
{
    /// <summary>
    /// Código da Fase - Phase Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_FASE", Order = 0)]
    [Required]
    public int CodFase { get; set; }

    /// <summary>
    /// Código do Evento - Event Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_EVENTO", Order = 1)]
    [Required]
    public int CodEvento { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Effective Start Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DATA_INIVIG_REFAEV", TypeName = "date", Order = 2)]
    [Required]
    public DateTime DataInivigRefaev { get; set; }

    /// <summary>
    /// Data de Término de Vigência - Effective End Date
    /// </summary>
    [Column("DATA_TERVIG_REFAEV", TypeName = "date")]
    public DateTime? DataTervigRefaev { get; set; }

    /// <summary>
    /// Indicador de Alteração de Fase - Phase Change Indicator
    /// '1' = Opens phase (abertura)
    /// '2' = Closes phase (fechamento)
    /// </summary>
    [Column("IND_ALTERACAO_FASE")]
    [Required]
    [StringLength(1)]
    [RegularExpression("^[12]$", ErrorMessage = "Phase Indicator must be '1' (open) or '2' (close)")]
    public string IndAlteracaoFase { get; set; } = string.Empty;

    /// <summary>
    /// Nome da Fase - Phase Name
    /// </summary>
    [Column("NOME_FASE")]
    [StringLength(100)]
    public string? NomeFase { get; set; }

    /// <summary>
    /// Nome do Evento - Event Name
    /// </summary>
    [Column("NOME_EVENTO")]
    [StringLength(100)]
    public string? NomeEvento { get; set; }

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

    /// <summary>
    /// Check if this relationship opens a phase
    /// </summary>
    [NotMapped]
    public bool OpensPhase => IndAlteracaoFase == "1";

    /// <summary>
    /// Check if this relationship closes a phase
    /// </summary>
    [NotMapped]
    public bool ClosesPhase => IndAlteracaoFase == "2";

    /// <summary>
    /// Check if configuration is valid for date
    /// </summary>
    public bool IsValidForDate(DateTime date)
    {
        if (date < DataInivigRefaev)
            return false;

        if (DataTervigRefaev.HasValue && date > DataTervigRefaev.Value)
            return false;

        return Ativo;
    }
}
