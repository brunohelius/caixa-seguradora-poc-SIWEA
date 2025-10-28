namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Response received from CNOUA, SIPUA, SIMDA validation services
/// </summary>
public class ExternalValidationResponse
{
    /// <summary>
    /// Response code ('00000000' = success, others = error)
    /// </summary>
    public string Ezert8 { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive error message in Portuguese (if Ezert8 != '00000000')
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Computed property: Ezert8 == '00000000'
    /// </summary>
    public bool IsSuccess => Ezert8 == "00000000";

    /// <summary>
    /// Service name ('CNOUA', 'SIPUA', 'SIMDA')
    /// </summary>
    public string ValidationService { get; set; } = string.Empty;

    /// <summary>
    /// When request was sent
    /// </summary>
    public DateTime RequestTimestamp { get; set; }

    /// <summary>
    /// When response received
    /// </summary>
    public DateTime ResponseTimestamp { get; set; }

    /// <summary>
    /// Response time measurement
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Maps EZERT8 code to Portuguese error message
    /// </summary>
    /// <returns>Portuguese error message or null if success</returns>
    public string? GetMappedErrorMessage()
    {
        return Ezert8 switch
        {
            "00000000" => null, // Success
            "EZERT8001" => "Contrato de consórcio inválido", // CONS-001
            "EZERT8002" => "Contrato cancelado", // CONS-002
            "EZERT8003" => "Grupo encerrado", // CONS-003
            "EZERT8004" => "Cota não contemplada", // CONS-004
            "EZERT8005" => "Beneficiário não autorizado", // CONS-005
            _ => "Serviço de validação indisponível" // SYS-005
        };
    }

    /// <summary>
    /// Maps EZERT8 code to error code (CONS-001 through CONS-005, SYS-005)
    /// </summary>
    /// <returns>Error code or null if success</returns>
    public string? GetErrorCode()
    {
        return Ezert8 switch
        {
            "00000000" => null,
            "EZERT8001" => "CONS-001",
            "EZERT8002" => "CONS-002",
            "EZERT8003" => "CONS-003",
            "EZERT8004" => "CONS-004",
            "EZERT8005" => "CONS-005",
            _ => "SYS-005"
        };
    }
}
