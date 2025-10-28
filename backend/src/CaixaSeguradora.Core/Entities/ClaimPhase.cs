using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim Phase - Phase tracking with dates (Legacy table: SI_SINISTRO_FASE)
/// Tracks claim processing phases including opening, closing, and effective dates.
/// </summary>
[Table("SI_SINISTRO_FASE")]
[Index(nameof(CodFase), nameof(DataAberturaSifa), Name = "IX_SI_SINISTRO_FASE_PhaseOpening")]
[Index(nameof(DataFechaSifa), Name = "IX_SI_SINISTRO_FASE_Closing")]
public class ClaimPhase
{
    /// <summary>
    /// Fonte do Protocolo - Protocol Source (Primary Key)
    /// </summary>
    [Key]
    [Column("FONTE", Order = 0)]
    [Required]
    public int Fonte { get; set; }

    /// <summary>
    /// Número do Protocolo - Protocol Number (Primary Key)
    /// </summary>
    [Key]
    [Column("PROTSINI", Order = 1)]
    [Required]
    public int Protsini { get; set; }

    /// <summary>
    /// Dígito de Controle - Check Digit (Primary Key)
    /// </summary>
    [Key]
    [Column("DAC", Order = 2)]
    [Required]
    public int Dac { get; set; }

    /// <summary>
    /// Código da Fase - Phase Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_FASE", Order = 3)]
    [Required]
    public int CodFase { get; set; }

    /// <summary>
    /// Código do Evento - Event Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_EVENTO", Order = 4)]
    [Required]
    public int CodEvento { get; set; }

    /// <summary>
    /// Número da Ocorrência - Event Occurrence (Primary Key)
    /// </summary>
    [Key]
    [Column("NUM_OCORR_SINIACO", Order = 5)]
    [Required]
    public int NumOcorrSiniaco { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Effective Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DATA_INIVIG_REFAEV", TypeName = "date", Order = 6)]
    [Required]
    public DateTime DataInivigRefaev { get; set; }

    /// <summary>
    /// Data de Abertura da Fase - Phase Opening Date
    /// </summary>
    [Column("DATA_ABERTURA_SIFA", TypeName = "date")]
    [Required]
    public DateTime DataAberturaSifa { get; set; }

    /// <summary>
    /// Data de Fechamento da Fase - Phase Closing Date
    /// Default: '9999-12-31' for open phases
    /// </summary>
    [Column("DATA_FECHA_SIFA", TypeName = "date")]
    [Required]
    public DateTime DataFechaSifa { get; set; }

    /// <summary>
    /// Nome da Fase - Phase Name
    /// </summary>
    [Column("NOME_FASE")]
    [StringLength(100)]
    public string? NomeFase { get; set; }

    /// <summary>
    /// Observações - Observations
    /// </summary>
    [Column("OBSERVACOES")]
    [StringLength(1000)]
    public string? Observacoes { get; set; }

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
    [ForeignKey($"{nameof(Fonte)},{nameof(Protsini)},{nameof(Dac)}")]
    public virtual ClaimMaster? Claim { get; set; }

    /// <summary>
    /// Phase-event relationship configuration
    /// </summary>
    [ForeignKey($"{nameof(CodFase)},{nameof(CodEvento)},{nameof(DataInivigRefaev)}")]
    public virtual PhaseEventRelationship? PhaseEventRelationship { get; set; }

    /// <summary>
    /// Check if phase is currently open
    /// </summary>
    [NotMapped]
    public bool IsOpen => DataFechaSifa.Year == 9999;

    /// <summary>
    /// Phase duration in days (null if still open)
    /// </summary>
    [NotMapped]
    public int? DurationInDays
    {
        get
        {
            if (IsOpen)
                return null;

            return (DataFechaSifa - DataAberturaSifa).Days;
        }
    }

    /// <summary>
    /// Days the phase has been open (0 if closed)
    /// </summary>
    [NotMapped]
    public int DaysOpen
    {
        get
        {
            if (!IsOpen)
                return 0;

            return (DateTime.Now - DataAberturaSifa).Days;
        }
    }
}
