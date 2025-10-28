using System.ComponentModel.DataAnnotations;

namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Claim Search Criteria DTO
/// At least one complete criteria set must be provided:
/// - Protocol: Fonte + Protsini + Dac
/// - Claim Number: Orgsin + Rmosin + Numsin
/// - Leader Code: Codlider + Sinlid
/// </summary>
public class ClaimSearchCriteria
{
    /// <summary>
    /// Fonte do Protocolo - Protocol Source
    /// </summary>
    public int? Fonte { get; set; }

    /// <summary>
    /// Número do Protocolo - Protocol Number
    /// </summary>
    public int? Protsini { get; set; }

    /// <summary>
    /// Dígito de Controle - Check Digit (0-9)
    /// </summary>
    [Range(0, 9, ErrorMessage = "DAC deve estar entre 0 e 9")]
    public int? Dac { get; set; }

    /// <summary>
    /// Órgão do Sinistro - Claim Origin
    /// </summary>
    public int? Orgsin { get; set; }

    /// <summary>
    /// Ramo do Sinistro - Claim Branch
    /// </summary>
    public int? Rmosin { get; set; }

    /// <summary>
    /// Número do Sinistro - Claim Number
    /// </summary>
    public int? Numsin { get; set; }

    /// <summary>
    /// Código do Líder - Leader Code
    /// </summary>
    public int? Codlider { get; set; }

    /// <summary>
    /// Sinistro do Líder - Leader Claim Number
    /// </summary>
    public int? Sinlid { get; set; }

    /// <summary>
    /// Validates if at least one complete criteria set is provided
    /// </summary>
    public bool IsValid()
    {
        bool hasProtocol = Fonte.HasValue && Fonte > 0
                        && Protsini.HasValue && Protsini > 0
                        && Dac.HasValue && Dac >= 0;

        bool hasClaimNumber = Orgsin.HasValue && Orgsin > 0
                           && Rmosin.HasValue && Rmosin > 0
                           && Numsin.HasValue && Numsin > 0;

        bool hasLeaderCode = Codlider.HasValue && Codlider > 0
                          && Sinlid.HasValue && Sinlid > 0;

        return hasProtocol || hasClaimNumber || hasLeaderCode;
    }

    /// <summary>
    /// Gets the search type being used
    /// </summary>
    public string GetSearchType()
    {
        if (Fonte.HasValue && Fonte > 0 && Protsini.HasValue && Protsini > 0 && Dac.HasValue && Dac >= 0)
            return "Protocol";

        if (Orgsin.HasValue && Orgsin > 0 && Rmosin.HasValue && Rmosin > 0 && Numsin.HasValue && Numsin > 0)
            return "ClaimNumber";

        if (Codlider.HasValue && Codlider > 0 && Sinlid.HasValue && Sinlid > 0)
            return "LeaderCode";

        return "None";
    }
}
