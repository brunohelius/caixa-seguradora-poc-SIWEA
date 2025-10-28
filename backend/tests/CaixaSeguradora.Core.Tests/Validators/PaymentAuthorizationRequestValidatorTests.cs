using Xunit;
using FluentValidation.TestHelper;
using CaixaSeguradora.Core.Validators;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Tests.Validators;

/// <summary>
/// Unit tests for PaymentAuthorizationRequestValidator
/// Verifies business rules: BR-004, BR-006, BR-019, and VALIDATION-001 through VALIDATION-010
/// </summary>
public class PaymentAuthorizationRequestValidatorTests
{
    private readonly PaymentAuthorizationRequestValidator _validator;

    public PaymentAuthorizationRequestValidatorTests()
    {
        _validator = new PaymentAuthorizationRequestValidator();
    }

    #region BR-006: Composite Key Validation

    [Theory]
    [InlineData(0)]   // Minimum valid
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]  // Maximum valid
    public void Tipseg_ValidValues_PassesValidation(int tipseg)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Tipseg = tipseg;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Tipseg);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Tipseg_NegativeValues_FailsValidation(int tipseg)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Tipseg = tipseg;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tipseg)
            .WithErrorCode("BR-006")
            .WithErrorMessage("TIPSEG deve ser maior ou igual a 0");
    }

    [Theory]
    [InlineData(1)]   // Minimum valid
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(99)]  // Maximum valid
    public void Orgsin_ValidValues_PassesValidation(int orgsin)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Orgsin = orgsin;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Orgsin);
    }

    [Theory]
    [InlineData(0)]    // Below minimum
    [InlineData(-1)]
    [InlineData(100)]  // Above maximum
    [InlineData(999)]
    public void Orgsin_OutOfRangeValues_FailsValidation(int orgsin)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Orgsin = orgsin;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Orgsin)
            .WithErrorCode("BR-006")
            .WithErrorMessage("ORGSIN deve estar entre 1 e 99");
    }

    [Theory]
    [InlineData(0)]   // Minimum valid
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]  // Maximum valid
    public void Rmosin_ValidValues_PassesValidation(int rmosin)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Rmosin = rmosin;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rmosin);
    }

    [Theory]
    [InlineData(-1)]   // Below minimum
    [InlineData(-10)]
    [InlineData(100)]  // Above maximum
    [InlineData(999)]
    public void Rmosin_OutOfRangeValues_FailsValidation(int rmosin)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Rmosin = rmosin;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rmosin)
            .WithErrorCode("BR-006")
            .WithErrorMessage("RMOSIN deve estar entre 0 e 99");
    }

    [Theory]
    [InlineData(1)]        // Minimum valid
    [InlineData(1000)]
    [InlineData(500000)]
    [InlineData(999999)]   // Maximum valid
    public void Numsin_ValidValues_PassesValidation(int numsin)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Numsin = numsin;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Numsin);
    }

    [Theory]
    [InlineData(0)]         // Below minimum
    [InlineData(-1)]
    [InlineData(-1000)]
    [InlineData(1000000)]   // Above maximum
    [InlineData(9999999)]
    public void Numsin_OutOfRangeValues_FailsValidation(int numsin)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Numsin = numsin;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Numsin)
            .WithErrorCode("BR-006")
            .WithErrorMessage("NUMSIN deve estar entre 1 e 999999");
    }

    #endregion

    #region BR-004: Payment Type Validation

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void TipoPagamento_ValidValues_PassesValidation(int tipoPagamento)
    {
        // Arrange
        var request = CreateValidRequest();
        request.TipoPagamento = tipoPagamento;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TipoPagamento);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(99)]
    public void TipoPagamento_InvalidValues_FailsValidation(int tipoPagamento)
    {
        // Arrange
        var request = CreateValidRequest();
        request.TipoPagamento = tipoPagamento;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TipoPagamento)
            .WithErrorCode("BR-004")
            .WithErrorMessage("Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5");
    }

    #endregion

    #region VALIDATION-001 and VALIDATION-002: Amount Validation

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(100.50)]
    [InlineData(1000.99)]
    [InlineData(999999.99)]
    public void Amount_ValidPositiveValues_PassesValidation(decimal amount)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1.00)]
    [InlineData(-100.50)]
    public void Amount_ZeroOrNegative_FailsValidation(decimal amount)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorCode("VALIDATION-001")
            .WithErrorMessage("Valor do pagamento deve ser maior que zero");
    }

    [Theory]
    [InlineData(100.123)]   // 3 decimal places
    [InlineData(50.9999)]   // 4 decimal places
    [InlineData(1.12345)]   // 5 decimal places
    public void Amount_MoreThanTwoDecimalPlaces_FailsValidation(decimal amount)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = amount;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorCode("VALIDATION-002")
            .WithErrorMessage("Valor do pagamento deve ter no máximo 2 casas decimais");
    }

    #endregion

    #region VALIDATION-003: Currency Code Validation

    [Theory]
    [InlineData("BRL")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void CurrencyCode_ValidThreeCharacterCodes_PassesValidation(string currencyCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.CurrencyCode = currencyCode;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CurrencyCode);
    }

    [Fact]
    public void CurrencyCode_Empty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CurrencyCode = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyCode)
            .WithErrorMessage("Código da moeda é obrigatório");
    }

    [Theory]
    [InlineData("BR")]      // Too short
    [InlineData("B")]
    [InlineData("BRLX")]    // Too long
    [InlineData("DOLLAR")]
    public void CurrencyCode_InvalidLength_FailsValidation(string currencyCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.CurrencyCode = currencyCode;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyCode)
            .WithErrorCode("VALIDATION-003")
            .WithErrorMessage("Código da moeda deve ter 3 caracteres (ex: BRL, USD)");
    }

    #endregion

    #region VALIDATION-004 and VALIDATION-005: Required Fields

    [Fact]
    public void ProductCode_Empty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProductCode = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductCode)
            .WithErrorCode("VALIDATION-004")
            .WithErrorMessage("Código do produto é obrigatório");
    }

    [Fact]
    public void PaymentMethod_Empty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PaymentMethod = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod)
            .WithErrorCode("VALIDATION-005")
            .WithErrorMessage("Método de pagamento é obrigatório");
    }

    #endregion

    #region BR-019 and VALIDATION-006: Beneficiary Name Validation

    [Fact]
    public void BeneficiaryName_ValidName_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BeneficiaryName = "João da Silva";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BeneficiaryName);
    }

    [Fact]
    public void BeneficiaryName_Null_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BeneficiaryName = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BeneficiaryName);
    }

    [Fact]
    public void BeneficiaryName_EmptyString_PassesValidation()
    {
        // Arrange - Empty string is treated as null by the validator
        // The actual BR-019 validation (beneficiary required if TPSEGU != 0) happens in service layer
        var request = CreateValidRequest();
        request.BeneficiaryName = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert - Empty string doesn't trigger validation because of When() clause
        result.ShouldNotHaveValidationErrorFor(x => x.BeneficiaryName);
    }

    [Fact]
    public void BeneficiaryName_Whitespace_PassesValidation()
    {
        // Arrange - Whitespace is treated as null by IsNullOrWhiteSpace check
        var request = CreateValidRequest();
        request.BeneficiaryName = "   ";

        // Act
        var result = _validator.TestValidate(request);

        // Assert - Whitespace doesn't trigger validation because of When() clause
        result.ShouldNotHaveValidationErrorFor(x => x.BeneficiaryName);
    }

    [Fact]
    public void BeneficiaryName_TooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BeneficiaryName = new string('A', 101); // 101 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BeneficiaryName)
            .WithErrorCode("VALIDATION-006")
            .WithErrorMessage("Nome do beneficiário deve ter no máximo 100 caracteres");
    }

    [Fact]
    public void BeneficiaryName_Exactly100Characters_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BeneficiaryName = new string('A', 100); // Exactly 100 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BeneficiaryName);
    }

    #endregion

    #region VALIDATION-007: Operator ID Validation

    [Fact]
    public void RequestedBy_Empty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestedBy = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequestedBy)
            .WithErrorCode("VALIDATION-007")
            .WithErrorMessage("ID do operador é obrigatório (EZEUSRID)");
    }

    [Fact]
    public void RequestedBy_TooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestedBy = new string('A', 21); // 21 characters

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequestedBy)
            .WithErrorCode("VALIDATION-007")
            .WithErrorMessage("ID do operador deve ter no máximo 20 caracteres");
    }

    [Fact]
    public void RequestedBy_ValidLength_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RequestedBy = "OPERATOR123";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RequestedBy);
    }

    #endregion

    #region VALIDATION-008 and VALIDATION-009: External Validation Routing

    [Fact]
    public void ExternalValidationRequired_TrueWithRoutingService_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalValidationRequired = true;
        request.RoutingService = "CNOUA";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExternalValidationRequired);
    }

    [Fact]
    public void ExternalValidationRequired_FalseWithRoutingService_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalValidationRequired = false;
        request.RoutingService = "CNOUA";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExternalValidationRequired)
            .WithErrorCode("VALIDATION-008")
            .WithErrorMessage("ExternalValidationRequired deve ser true quando RoutingService é fornecido");
    }

    [Theory]
    [InlineData("CNOUA")]
    [InlineData("SIPUA")]
    [InlineData("SIMDA")]
    [InlineData(null)]
    public void RoutingService_ValidValues_PassesValidation(string? routingService)
    {
        // Arrange
        var request = CreateValidRequest();
        request.RoutingService = routingService;
        request.ExternalValidationRequired = routingService != null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoutingService);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("CNOUA_EXTRA")]
    [InlineData("")]
    [InlineData("cnoua")]  // Case sensitive
    public void RoutingService_InvalidValues_FailsValidation(string routingService)
    {
        // Arrange
        var request = CreateValidRequest();
        request.RoutingService = routingService;
        request.ExternalValidationRequired = true;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoutingService)
            .WithErrorCode("VALIDATION-009")
            .WithErrorMessage("RoutingService deve ser CNOUA, SIPUA, SIMDA, ou null (roteamento automático)");
    }

    #endregion

    #region VALIDATION-010: Contract Number Validation

    [Theory]
    [InlineData("SIPUA", 123456)]
    [InlineData("SIMDA", 1)]
    [InlineData("SIMDA", 999999)]
    public void NumContrato_ValidForSIPUAOrSIMDA_PassesValidation(string routingService, int numContrato)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalValidationRequired = true;
        request.RoutingService = routingService;
        request.NumContrato = numContrato;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NumContrato);
    }

    [Theory]
    [InlineData("SIPUA", 0)]
    [InlineData("SIPUA", -1)]
    [InlineData("SIMDA", 0)]
    [InlineData("SIMDA", -100)]
    public void NumContrato_ZeroOrNegativeForSIPUAOrSIMDA_FailsValidation(string routingService, int numContrato)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalValidationRequired = true;
        request.RoutingService = routingService;
        request.NumContrato = numContrato;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NumContrato)
            .WithErrorCode("VALIDATION-010")
            .WithErrorMessage("Número do contrato é obrigatório para validação SIPUA/SIMDA");
    }

    [Fact]
    public void NumContrato_ZeroForCNOUA_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalValidationRequired = true;
        request.RoutingService = "CNOUA";
        request.NumContrato = 0; // CNOUA doesn't require contract number

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NumContrato);
    }

    #endregion

    #region Happy Path Integration Tests

    [Fact]
    public void ValidRequest_AllFieldsValid_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidRequest_WithExternalValidation_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalValidationRequired = true;
        request.RoutingService = "CNOUA";
        request.NumContrato = 0; // CNOUA doesn't need contract number

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidRequest_WithBeneficiary_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BeneficiaryName = "Maria da Silva";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a valid PaymentAuthorizationRequest for testing purposes
    /// </summary>
    private static PaymentAuthorizationRequest CreateValidRequest()
    {
        return new PaymentAuthorizationRequest
        {
            // BR-006: Composite key
            Tipseg = 0,
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 123456,

            // BR-004: Payment type (1-5)
            TipoPagamento = 1,

            // VALIDATION-001, VALIDATION-002: Amount
            Amount = 1000.50m,

            // VALIDATION-003: Currency code
            CurrencyCode = "BRL",

            // VALIDATION-004: Product code
            ProductCode = "PROD123",

            // VALIDATION-005: Payment method
            PaymentMethod = "TRANSFERENCIA",

            // VALIDATION-007: Operator ID
            RequestedBy = "OPERATOR123",

            // External validation (optional)
            ExternalValidationRequired = false,
            RoutingService = null,
            NumContrato = 0,

            // Beneficiary (optional, BR-019)
            BeneficiaryName = null
        };
    }

    #endregion
}
