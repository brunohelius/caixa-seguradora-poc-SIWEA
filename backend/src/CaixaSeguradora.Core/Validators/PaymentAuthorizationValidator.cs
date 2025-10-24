using FluentValidation;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Validators;

/// <summary>
/// Validator for payment authorization requests
/// </summary>
public class PaymentAuthorizationValidator : AbstractValidator<PaymentAuthorizationRequest>
{
    private static readonly string[] ValidCurrencies = { "BRL", "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD" };
    private static readonly string[] ValidPaymentMethods = { "TRANSFER", "CHECK", "CREDIT", "DEBIT", "PIX", "TED", "DOC" };
    private static readonly string[] ValidAccountTypes = { "CHECKING", "SAVINGS", "PAYMENT" };

    public PaymentAuthorizationValidator()
    {
        RuleFor(x => x.ClaimId)
            .GreaterThan(0)
            .WithMessage("Claim ID must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Amount cannot exceed 10,000,000");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required")
            .Must(code => ValidCurrencies.Contains(code.ToUpperInvariant()))
            .WithMessage($"Currency code must be one of: {string.Join(", ", ValidCurrencies)}");

        RuleFor(x => x.TargetCurrencyCode)
            .Must((request, target) => string.IsNullOrEmpty(target) || ValidCurrencies.Contains(target.ToUpperInvariant()))
            .WithMessage($"Target currency code must be one of: {string.Join(", ", ValidCurrencies)}");

        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .WithMessage("Product code is required")
            .MaximumLength(20)
            .WithMessage("Product code cannot exceed 20 characters");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required")
            .Must(method => ValidPaymentMethods.Contains(method.ToUpperInvariant()))
            .WithMessage($"Payment method must be one of: {string.Join(", ", ValidPaymentMethods)}");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty()
            .WithMessage("Beneficiary name is required")
            .MinimumLength(3)
            .WithMessage("Beneficiary name must have at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Beneficiary name cannot exceed 100 characters");

        RuleFor(x => x.BeneficiaryTaxId)
            .NotEmpty()
            .WithMessage("Beneficiary tax ID (CPF/CNPJ) is required")
            .Must(BeValidTaxId)
            .WithMessage("Invalid tax ID format (must be CPF: 11 digits or CNPJ: 14 digits)");

        When(x => x.PaymentMethod.ToUpperInvariant() == "TRANSFER" ||
                  x.PaymentMethod.ToUpperInvariant() == "TED" ||
                  x.PaymentMethod.ToUpperInvariant() == "DOC", () =>
        {
            RuleFor(x => x.BankCode)
                .NotEmpty()
                .WithMessage("Bank code is required for transfer payments")
                .Matches(@"^\d{3}$")
                .WithMessage("Bank code must be 3 digits");

            RuleFor(x => x.BranchCode)
                .NotEmpty()
                .WithMessage("Branch code is required for transfer payments")
                .Matches(@"^\d{4}$")
                .WithMessage("Branch code must be 4 digits");

            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required for transfer payments")
                .MaximumLength(20)
                .WithMessage("Account number cannot exceed 20 characters");

            RuleFor(x => x.AccountType)
                .NotEmpty()
                .WithMessage("Account type is required for transfer payments")
                .Must(type => ValidAccountTypes.Contains(type!.ToUpperInvariant()))
                .WithMessage($"Account type must be one of: {string.Join(", ", ValidAccountTypes)}");
        });

        RuleFor(x => x.RequestedBy)
            .NotEmpty()
            .WithMessage("Requested by is required")
            .MaximumLength(50)
            .WithMessage("Requested by cannot exceed 50 characters");

        RuleFor(x => x.RequestedAt)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Request timestamp cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    /// <summary>
    /// Validates Brazilian CPF or CNPJ format
    /// </summary>
    private bool BeValidTaxId(string taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            return false;

        // Remove non-digit characters
        var digits = new string(taxId.Where(char.IsDigit).ToArray());

        // CPF: 11 digits, CNPJ: 14 digits
        return digits.Length == 11 || digits.Length == 14;
    }
}
