using System;

namespace CaixaSeguradora.Core.Exceptions;

/// <summary>
/// Exception thrown when a claim is not found
/// </summary>
public class ClaimNotFoundException : Exception
{
    public string ErrorCode { get; }

    public ClaimNotFoundException(string message)
        : base(message)
    {
        ErrorCode = "SINISTRO_NAO_ENCONTRADO";
    }

    public ClaimNotFoundException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ClaimNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "SINISTRO_NAO_ENCONTRADO";
    }

    /// <summary>
    /// Creates exception for protocol not found
    /// </summary>
    public static ClaimNotFoundException ForProtocol(int fonte, int protsini, int dac)
    {
        string protocol = $"{fonte:D3}/{protsini:D7}-{dac}";
        return new ClaimNotFoundException($"DOCUMENTO {protocol} NAO CADASTRADO");
    }

    /// <summary>
    /// Creates exception for claim number not found
    /// </summary>
    public static ClaimNotFoundException ForClaimNumber(int orgsin, int rmosin, int numsin)
    {
        string claimNumber = $"{orgsin}/{rmosin}/{numsin}";
        return new ClaimNotFoundException($"Sinistro {claimNumber} não encontrado");
    }

    /// <summary>
    /// Creates exception for leader code not found
    /// </summary>
    public static ClaimNotFoundException ForLeaderCode(int codlider, int sinlid)
    {
        return new ClaimNotFoundException($"Sinistro do líder {codlider}/{sinlid} não encontrado");
    }
}
