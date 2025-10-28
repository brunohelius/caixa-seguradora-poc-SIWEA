using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim Accompaniment - Workflow event tracking (Legacy table: SI_ACOMPANHA_SINI)
/// Records claim events, activities, and workflow progression.
/// </summary>
[Table("SI_ACOMPANHA_SINI")]
[Index(nameof(CodEvento), nameof(DataMovtoSiniaco), Name = "IX_SI_ACOMPANHA_EventDate")]
[Index(nameof(CodUsuario), nameof(DataMovtoSiniaco), Name = "IX_SI_ACOMPANHA_UserDate")]
public class ClaimAccompaniment
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
    /// Código do Evento - Event Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_EVENTO", Order = 3)]
    [Required]
    public int CodEvento { get; set; }

    /// <summary>
    /// Data do Movimento - Transaction Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DATA_MOVTO_SINIACO", TypeName = "date", Order = 4)]
    [Required]
    public DateTime DataMovtoSiniaco { get; set; }

    /// <summary>
    /// Número da Ocorrência - Occurrence Number
    /// </summary>
    [Column("NUM_OCORR_SINIACO")]
    [Required]
    public int NumOcorrSiniaco { get; set; }

    /// <summary>
    /// Descrição Complementar - Complementary Description
    /// </summary>
    [Column("DESCR_COMPLEMENTAR")]
    [StringLength(500)]
    public string? DescrComplementar { get; set; }

    /// <summary>
    /// Código do Usuário - User ID
    /// </summary>
    [Column("COD_USUARIO")]
    [Required]
    [StringLength(50)]
    public string CodUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Hora do Evento - Event Time
    /// Format: HHmmss
    /// </summary>
    [Column("HORA_EVENTO")]
    [StringLength(6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Time must be in HHmmss format")]
    public string? HoraEvento { get; set; }

    /// <summary>
    /// Nome do Evento - Event Name
    /// </summary>
    [Column("NOME_EVENTO")]
    [StringLength(100)]
    public string? NomeEvento { get; set; }

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
    /// Combined event timestamp
    /// </summary>
    [NotMapped]
    public DateTime EventTimestamp
    {
        get
        {
            if (string.IsNullOrEmpty(HoraEvento) || HoraEvento.Length != 6)
                return DataMovtoSiniaco;

            int hour = int.Parse(HoraEvento.Substring(0, 2));
            int minute = int.Parse(HoraEvento.Substring(2, 2));
            int second = int.Parse(HoraEvento.Substring(4, 2));

            return DataMovtoSiniaco.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
        }
    }
}
