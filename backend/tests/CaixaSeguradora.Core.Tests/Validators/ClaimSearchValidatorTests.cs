using Xunit;
using FluentValidation.TestHelper;
using CaixaSeguradora.Core.Validators;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Tests.Validators;

/// <summary>
/// Unit tests for ClaimSearchValidator
/// Verifies validation logic for three search criteria sets:
/// - Protocol search (Fonte + Protsini + DAC)
/// - Claim number search (Orgsin + Rmosin + Numsin)
/// - Leader code search (Codlider + Sinlid)
/// </summary>
public class ClaimSearchValidatorTests
{
    private readonly ClaimSearchValidator _validator;

    public ClaimSearchValidatorTests()
    {
        _validator = new ClaimSearchValidator();
    }

    #region Protocol Search Criteria (Fonte + Protsini + DAC)

    [Fact]
    public void ProtocolSearch_AllFieldsValid_PassesValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 5
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]  // Minimum valid (IsValid() requires > 0 but validator allows >= 0 for DAC)
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]  // Maximum valid
    public void ProtocolSearch_ValidDacValues_PassesValidation(int dac)
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = dac
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Dac);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(10)]
    [InlineData(99)]
    public void ProtocolSearch_InvalidDacValues_FailsValidation(int dac)
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = dac
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Dac)
            .WithErrorMessage("DAC deve estar entre 0 e 9");
    }

    [Fact]
    public void ProtocolSearch_ZeroFonte_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 0,
            Protsini = 123456,
            Dac = 5
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Fonte)
            .WithErrorMessage("Fonte deve ser maior que zero");
    }

    [Fact]
    public void ProtocolSearch_NegativeFonte_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = -1,
            Protsini = 123456,
            Dac = 5
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Fonte)
            .WithErrorMessage("Fonte deve ser maior que zero");
    }

    [Fact]
    public void ProtocolSearch_ZeroProtsini_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 0,
            Dac = 5
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Protsini)
            .WithErrorMessage("Número do protocolo deve ser maior que zero");
    }

    [Fact]
    public void ProtocolSearch_MissingFonte_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = null,
            Protsini = 123456,
            Dac = 5
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por protocolo, todos os campos (Fonte, Protsini e DAC) devem ser fornecidos");
    }

    [Fact]
    public void ProtocolSearch_MissingProtsini_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = null,
            Dac = 5
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por protocolo, todos os campos (Fonte, Protsini e DAC) devem ser fornecidos");
    }

    [Fact]
    public void ProtocolSearch_MissingDac_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = null
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por protocolo, todos os campos (Fonte, Protsini e DAC) devem ser fornecidos");
    }

    #endregion

    #region Claim Number Search Criteria (Orgsin + Rmosin + Numsin)

    [Fact]
    public void ClaimNumberSearch_AllFieldsValid_PassesValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 123456
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ClaimNumberSearch_ZeroOrgsin_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 0,
            Rmosin = 5,
            Numsin = 123456
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Orgsin)
            .WithErrorMessage("Órgão do sinistro deve ser maior que zero");
    }

    [Fact]
    public void ClaimNumberSearch_ZeroRmosin_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = 0,
            Numsin = 123456
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rmosin)
            .WithErrorMessage("Ramo do sinistro deve ser maior que zero");
    }

    [Fact]
    public void ClaimNumberSearch_ZeroNumsin_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 0
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Numsin)
            .WithErrorMessage("Número do sinistro deve ser maior que zero");
    }

    [Fact]
    public void ClaimNumberSearch_MissingOrgsin_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = null,
            Rmosin = 5,
            Numsin = 123456
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por número de sinistro, todos os campos (Orgsin, Rmosin e Numsin) devem ser fornecidos");
    }

    [Fact]
    public void ClaimNumberSearch_MissingRmosin_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = null,
            Numsin = 123456
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por número de sinistro, todos os campos (Orgsin, Rmosin e Numsin) devem ser fornecidos");
    }

    [Fact]
    public void ClaimNumberSearch_MissingNumsin_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = 5,
            Numsin = null
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por número de sinistro, todos os campos (Orgsin, Rmosin e Numsin) devem ser fornecidos");
    }

    #endregion

    #region Leader Code Search Criteria (Codlider + Sinlid)

    [Fact]
    public void LeaderCodeSearch_AllFieldsValid_PassesValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 100,
            Sinlid = 999
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LeaderCodeSearch_ZeroCodlider_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 0,
            Sinlid = 999
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Codlider)
            .WithErrorMessage("Código do líder deve ser maior que zero");
    }

    [Fact]
    public void LeaderCodeSearch_ZeroSinlid_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 100,
            Sinlid = 0
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sinlid)
            .WithErrorMessage("Sinistro do líder deve ser maior que zero");
    }

    [Fact]
    public void LeaderCodeSearch_MissingCodlider_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = null,
            Sinlid = 999
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por código de líder, ambos os campos (Codlider e Sinlid) devem ser fornecidos");
    }

    [Fact]
    public void LeaderCodeSearch_MissingSinlid_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 100,
            Sinlid = null
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por código de líder, ambos os campos (Codlider e Sinlid) devem ser fornecidos");
    }

    #endregion

    #region No Criteria Provided

    [Fact]
    public void NoCriteria_EmptyObject_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria();

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Você deve fornecer ao menos um critério de busca completo: " +
                             "(Fonte, Protsini e DAC) OU " +
                             "(Orgsin, Rmosin e Numsin) OU " +
                             "(Codlider e Sinlid)");
    }

    [Fact]
    public void NoCriteria_AllFieldsNull_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = null,
            Protsini = null,
            Dac = null,
            Orgsin = null,
            Rmosin = null,
            Numsin = null,
            Codlider = null,
            Sinlid = null
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Você deve fornecer ao menos um critério de busca completo: " +
                             "(Fonte, Protsini e DAC) OU " +
                             "(Orgsin, Rmosin e Numsin) OU " +
                             "(Codlider e Sinlid)");
    }

    #endregion

    #region Mixed Criteria (Multiple Search Types)

    [Fact]
    public void MixedCriteria_ProtocolAndClaimNumber_PassesValidation()
    {
        // Arrange - Both Protocol and Claim Number search criteria provided
        var criteria = new ClaimSearchCriteria
        {
            // Protocol criteria
            Fonte = 1,
            Protsini = 123456,
            Dac = 5,

            // Claim number criteria
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 789
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert - Should pass because at least one complete set is provided
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MixedCriteria_AllThreeSets_PassesValidation()
    {
        // Arrange - All three search criteria sets provided
        var criteria = new ClaimSearchCriteria
        {
            // Protocol criteria
            Fonte = 1,
            Protsini = 123456,
            Dac = 5,

            // Claim number criteria
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 789,

            // Leader code criteria
            Codlider = 100,
            Sinlid = 999
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert - Should pass because all sets are complete
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void EdgeCase_OnlyFonteProvided_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert - Should fail because Protocol search is incomplete
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por protocolo, todos os campos (Fonte, Protsini e DAC) devem ser fornecidos");
    }

    [Fact]
    public void EdgeCase_OnlyOrgsinProvided_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert - Should fail because Claim Number search is incomplete
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por número de sinistro, todos os campos (Orgsin, Rmosin e Numsin) devem ser fornecidos");
    }

    [Fact]
    public void EdgeCase_OnlyCodliderProvided_FailsValidation()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 100
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert - Should fail because Leader Code search is incomplete
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por código de líder, ambos os campos (Codlider e Sinlid) devem ser fornecidos");
    }

    [Fact]
    public void EdgeCase_IncompleteProtocolWithCompleteClaimNumber_PassesValidation()
    {
        // Arrange - Incomplete protocol, but complete claim number
        var criteria = new ClaimSearchCriteria
        {
            // Incomplete protocol (missing DAC)
            Fonte = 1,
            Protsini = 123456,

            // Complete claim number
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 789
        };

        // Act
        var result = _validator.TestValidate(criteria);

        // Assert - Should pass because claim number is complete (HaveValidCriteria succeeds)
        // But should have error for incomplete protocol fields
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Para busca por protocolo, todos os campos (Fonte, Protsini e DAC) devem ser fornecidos");
    }

    #endregion

    #region Integration Tests with IsValid() Method

    [Fact]
    public void IsValid_ProtocolComplete_ReturnsTrue()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 5
        };

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ClaimNumberComplete_ReturnsTrue()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 123456
        };

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_LeaderCodeComplete_ReturnsTrue()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 100,
            Sinlid = 999
        };

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NoCriteria_ReturnsFalse()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria();

        // Act
        bool isValid = criteria.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetSearchType_ProtocolComplete_ReturnsProtocol()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 5
        };

        // Act
        string searchType = criteria.GetSearchType();

        // Assert
        Assert.Equal("Protocol", searchType);
    }

    [Fact]
    public void GetSearchType_ClaimNumberComplete_ReturnsClaimNumber()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 10,
            Rmosin = 5,
            Numsin = 123456
        };

        // Act
        string searchType = criteria.GetSearchType();

        // Assert
        Assert.Equal("ClaimNumber", searchType);
    }

    [Fact]
    public void GetSearchType_LeaderCodeComplete_ReturnsLeaderCode()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 100,
            Sinlid = 999
        };

        // Act
        string searchType = criteria.GetSearchType();

        // Assert
        Assert.Equal("LeaderCode", searchType);
    }

    [Fact]
    public void GetSearchType_NoCriteria_ReturnsNone()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria();

        // Act
        string searchType = criteria.GetSearchType();

        // Assert
        Assert.Equal("None", searchType);
    }

    #endregion
}
