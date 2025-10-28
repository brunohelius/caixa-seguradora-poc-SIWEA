namespace CaixaSeguradora.Core.Enums;

/// <summary>
/// T087: ValidationRoute Enum
/// Defines the external validation service routes for different product types
/// </summary>
public enum ValidationRoute
{
    /// <summary>
    /// No validation required
    /// </summary>
    None = 0,

    /// <summary>
    /// CNOUA - Validation for consortium products (codes 6814, 7701, 7709)
    /// </summary>
    CNOUA = 1,

    /// <summary>
    /// SIPUA - Validation for EFP (Entidade Fechada de Previdência) contracts
    /// NUM_CONTRATO > 0 in EF_CONTR_SEG_HABIT table
    /// </summary>
    SIPUA = 2,

    /// <summary>
    /// SIMDA - Validation for HB (Habitação) contracts
    /// NUM_CONTRATO = 0 or not in EF_CONTR_SEG_HABIT table
    /// </summary>
    SIMDA = 3
}

/// <summary>
/// Extension methods for ValidationRoute enum
/// </summary>
public static class ValidationRouteExtensions
{
    /// <summary>
    /// Gets the service name for the validation route
    /// </summary>
    public static string GetServiceName(this ValidationRoute route)
    {
        return route switch
        {
            ValidationRoute.CNOUA => "CNOUA",
            ValidationRoute.SIPUA => "SIPUA",
            ValidationRoute.SIMDA => "SIMDA",
            ValidationRoute.None => "None",
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Invalid validation route")
        };
    }

    /// <summary>
    /// Gets a Portuguese description of the validation route
    /// </summary>
    public static string GetDescription(this ValidationRoute route)
    {
        return route switch
        {
            ValidationRoute.CNOUA => "Validação de produtos de consórcio via CNOUA",
            ValidationRoute.SIPUA => "Validação de contratos EFP via SIPUA",
            ValidationRoute.SIMDA => "Validação de contratos HB via SIMDA",
            ValidationRoute.None => "Sem validação externa",
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Invalid validation route")
        };
    }

    /// <summary>
    /// Checks if the route requires external validation
    /// </summary>
    public static bool IsExternal(this ValidationRoute route)
    {
        return route != ValidationRoute.None;
    }

    /// <summary>
    /// Gets the base URL configuration key for the validation service
    /// </summary>
    public static string GetConfigurationKey(this ValidationRoute route)
    {
        return route switch
        {
            ValidationRoute.CNOUA => "ExternalServices:CNOUA:BaseUrl",
            ValidationRoute.SIPUA => "ExternalServices:SIPUA:BaseUrl",
            ValidationRoute.SIMDA => "ExternalServices:SIMDA:BaseUrl",
            ValidationRoute.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Invalid validation route")
        };
    }

    /// <summary>
    /// Gets the timeout configuration key for the validation service
    /// </summary>
    public static string GetTimeoutConfigurationKey(this ValidationRoute route)
    {
        return route switch
        {
            ValidationRoute.CNOUA => "ExternalServices:CNOUA:TimeoutSeconds",
            ValidationRoute.SIPUA => "ExternalServices:SIPUA:TimeoutSeconds",
            ValidationRoute.SIMDA => "ExternalServices:SIMDA:TimeoutSeconds",
            ValidationRoute.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Invalid validation route")
        };
    }

    /// <summary>
    /// Maps EZERT8 error codes to Portuguese error messages (for CNOUA)
    /// </summary>
    public static string MapCNOUAErrorCode(string ezert8Code)
    {
        return ezert8Code switch
        {
            "00000000" => "Validação bem-sucedida",
            "EZERT8001" => "Contrato de consórcio inválido",
            "EZERT8002" => "Contrato cancelado",
            "EZERT8003" => "Grupo encerrado",
            "EZERT8004" => "Participante não encontrado",
            "EZERT8005" => "Cota contemplada pendente de quitação",
            "EZERT8006" => "Documentação incompleta",
            "EZERT8007" => "Valor excede o limite permitido",
            "EZERT8008" => "Período de carência não cumprido",
            "EZERT8009" => "Sinistro já indenizado",
            "EZERT8010" => "Cobertura não contratada",
            _ => $"Erro de validação CNOUA: {ezert8Code}"
        };
    }

    /// <summary>
    /// Gets the product codes associated with each validation route
    /// </summary>
    public static int[] GetAssociatedProductCodes(this ValidationRoute route)
    {
        return route switch
        {
            ValidationRoute.CNOUA => new[] { 6814, 7701, 7709 },
            ValidationRoute.SIPUA => Array.Empty<int>(), // Determined by contract
            ValidationRoute.SIMDA => Array.Empty<int>(), // Determined by contract
            ValidationRoute.None => Array.Empty<int>(),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Invalid validation route")
        };
    }
}
