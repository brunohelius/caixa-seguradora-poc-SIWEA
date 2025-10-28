namespace CaixaSeguradora.Core.Enums;

/// <summary>
/// Severity level for business rule violations
/// </summary>
public enum Severity
{
    /// <summary>
    /// Blocks transaction, must be resolved
    /// </summary>
    Critical = 1,

    /// <summary>
    /// Logged but doesn't block (for audit trail)
    /// </summary>
    Warning = 2
}
