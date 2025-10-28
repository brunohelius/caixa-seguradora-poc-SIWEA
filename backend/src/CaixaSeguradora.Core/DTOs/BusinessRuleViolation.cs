using CaixaSeguradora.Core.Enums;

namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Represents a single business rule validation failure
/// Validation Rules:
/// - Required: RuleId, RuleName, ErrorCode, ErrorMessage, Severity
/// - RuleId must match pattern "BR-\d{3}" (BR-001 through BR-099)
/// - ErrorCode must match pattern "VAL-\d{3}" or "CONS-\d{3}" or "SYS-\d{3}"
/// </summary>
public class BusinessRuleViolation
{
    /// <summary>
    /// Business rule identifier (e.g., "BR-019")
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable rule name (e.g., "Beneficiary Required")
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Error code from ErrorMessages.pt-BR (e.g., "VAL-007")
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Portuguese error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Actual value that violated rule (for diagnostics)
    /// </summary>
    public object? FailedValue { get; set; }

    /// <summary>
    /// Expected value or condition (for diagnostics)
    /// </summary>
    public object? ExpectedValue { get; set; }

    /// <summary>
    /// Critical (blocks transaction) vs Warning (logged only)
    /// </summary>
    public Severity Severity { get; set; }

    /// <summary>
    /// Factory method for creating a critical violation
    /// </summary>
    public static BusinessRuleViolation CreateCritical(
        string ruleId,
        string ruleName,
        string errorCode,
        string errorMessage,
        object? failedValue = null,
        object? expectedValue = null)
    {
        return new BusinessRuleViolation
        {
            RuleId = ruleId,
            RuleName = ruleName,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            FailedValue = failedValue,
            ExpectedValue = expectedValue,
            Severity = Severity.Critical
        };
    }

    /// <summary>
    /// Factory method for creating a warning violation
    /// </summary>
    public static BusinessRuleViolation CreateWarning(
        string ruleId,
        string ruleName,
        string errorCode,
        string errorMessage,
        object? failedValue = null,
        object? expectedValue = null)
    {
        return new BusinessRuleViolation
        {
            RuleId = ruleId,
            RuleName = ruleName,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            FailedValue = failedValue,
            ExpectedValue = expectedValue,
            Severity = Severity.Warning
        };
    }
}
