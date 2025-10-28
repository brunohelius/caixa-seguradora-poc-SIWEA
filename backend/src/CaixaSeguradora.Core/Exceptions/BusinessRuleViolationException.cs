using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Enums;

namespace CaixaSeguradora.Core.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated during validation
/// Contains structured information about the violation for client consumption
/// </summary>
public class BusinessRuleViolationException : Exception
{
    /// <summary>
    /// List of business rule violations
    /// </summary>
    public List<BusinessRuleViolation> Violations { get; }

    /// <summary>
    /// Error code for the business rule violation
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Business rule ID (for backward compatibility)
    /// </summary>
    public string? RuleId { get; }

    /// <summary>
    /// Creates a new BusinessRuleViolationException with a single violation
    /// </summary>
    /// <param name="errorCode">Error code (VAL-001, CONS-001, etc.)</param>
    /// <param name="message">Error message in Portuguese</param>
    /// <param name="ruleId">Business rule ID (BR-001, BR-002, etc.)</param>
    public BusinessRuleViolationException(
        string errorCode,
        string message,
        string? ruleId = null)
        : base(message)
    {
        ErrorCode = errorCode;
        RuleId = ruleId;
        Violations = new List<BusinessRuleViolation>
        {
            new BusinessRuleViolation
            {
                RuleId = ruleId ?? "UNKNOWN",
                RuleName = "Business Rule Violation",
                ErrorCode = errorCode,
                ErrorMessage = message,
                Severity = Severity.Critical
            }
        };
    }

    /// <summary>
    /// Creates a new BusinessRuleViolationException with multiple violations
    /// </summary>
    /// <param name="violations">List of business rule violations</param>
    public BusinessRuleViolationException(List<BusinessRuleViolation> violations)
        : base(violations.FirstOrDefault()?.ErrorMessage ?? "Business rule validation failed")
    {
        Violations = violations;
        ErrorCode = violations.FirstOrDefault()?.ErrorCode ?? "VALIDATION_ERROR";
        RuleId = violations.FirstOrDefault()?.RuleId;
    }

    /// <summary>
    /// Creates a new BusinessRuleViolationException with error code and violations
    /// </summary>
    /// <param name="errorCode">Error code</param>
    /// <param name="violations">List of business rule violations</param>
    public BusinessRuleViolationException(string errorCode, List<BusinessRuleViolation> violations)
        : base(violations.FirstOrDefault()?.ErrorMessage ?? "Business rule validation failed")
    {
        ErrorCode = errorCode;
        RuleId = violations.FirstOrDefault()?.RuleId;
        Violations = violations;
    }

    /// <summary>
    /// Gets all error messages concatenated
    /// </summary>
    /// <returns>All messages separated by semicolons</returns>
    public string GetAllMessages()
    {
        return string.Join("; ", Violations.Select(v => v.ErrorMessage));
    }

    /// <summary>
    /// Gets all rule IDs
    /// </summary>
    /// <returns>List of rule IDs</returns>
    public List<string> GetRuleIds()
    {
        return Violations
            .Where(v => !string.IsNullOrWhiteSpace(v.RuleId))
            .Select(v => v.RuleId!)
            .ToList();
    }
}
