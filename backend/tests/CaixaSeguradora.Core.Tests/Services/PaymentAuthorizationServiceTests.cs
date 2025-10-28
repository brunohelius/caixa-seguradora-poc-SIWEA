using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Core.ValueObjects;
using CaixaSeguradora.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ValidationResult = FluentValidation.Results.ValidationResult;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace CaixaSeguradora.Core.Tests.Services;

/// <summary>
/// T126-T155 [US4, BR-004, BR-005, BR-019, BR-023, BR-067] - Payment Authorization Service Unit Tests
/// Tests the 8-step payment authorization pipeline with atomic transaction handling
/// </summary>
public class PaymentAuthorizationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ExternalServiceRouter> _mockExternalServiceRouter;
    private readonly Mock<ICurrencyConversionService> _mockCurrencyConversionService;
    private readonly Mock<IPhaseManagementService> _mockPhaseManagementService;
    private readonly Mock<IValidator<PaymentAuthorizationRequest>> _mockValidator;
    private readonly Mock<ILogger<PaymentAuthorizationService>> _mockLogger;
    private readonly PaymentAuthorizationService _service;
    private readonly Mock<IClaimRepository> _mockClaimRepository;
    private readonly Mock<IClaimHistoryRepository> _mockClaimHistoryRepository;
    private readonly Mock<IRepository<ClaimAccompaniment>> _mockClaimAccompanimentRepository;
    private readonly Mock<IRepository<SystemControl>> _mockSystemControlRepository;

    public PaymentAuthorizationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExternalServiceRouter = new Mock<ExternalServiceRouter>(
            Mock.Of<ILogger<ExternalServiceRouter>>());
        _mockCurrencyConversionService = new Mock<ICurrencyConversionService>();
        _mockPhaseManagementService = new Mock<IPhaseManagementService>();
        _mockValidator = new Mock<IValidator<PaymentAuthorizationRequest>>();
        _mockLogger = new Mock<ILogger<PaymentAuthorizationService>>();
        _mockClaimRepository = new Mock<IClaimRepository>();
        _mockClaimHistoryRepository = new Mock<IClaimHistoryRepository>();
        _mockClaimAccompanimentRepository = new Mock<IRepository<ClaimAccompaniment>>();
        _mockSystemControlRepository = new Mock<IRepository<SystemControl>>();

        _mockUnitOfWork.Setup(u => u.Claims).Returns(_mockClaimRepository.Object);
        _mockUnitOfWork.Setup(u => u.ClaimHistories).Returns(_mockClaimHistoryRepository.Object);
        _mockUnitOfWork.Setup(u => u.ClaimAccompaniments).Returns(_mockClaimAccompanimentRepository.Object);
        _mockUnitOfWork.Setup(u => u.SystemControls).Returns(_mockSystemControlRepository.Object);

        _service = new PaymentAuthorizationService(
            _mockUnitOfWork.Object,
            _mockExternalServiceRouter.Object,
            _mockCurrencyConversionService.Object,
            _mockPhaseManagementService.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task AuthorizePaymentAsync_WithValidRequest_ReturnsApproved()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("APPROVED", result.Status);
        Assert.True(result.Sucesso);
        Assert.False(result.RollbackOccurred);
        Assert.Equal(request.Amount, result.AuthorizedAmount);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithValidRequest_InsertsHistoryRecord()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        _mockClaimHistoryRepository.Verify(r => r.AddAsync(
            It.Is<ClaimHistory>(h => h.Operacao == 1098 && h.Valpri == request.Amount),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(3));
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithValidRequest_UpdatesClaimMaster()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();
        var originalTotpag = claim.Totpag;
        var originalOcorhist = claim.Ocorhist;

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal(originalTotpag + request.Amount, claim.Totpag);
        Assert.Equal(originalOcorhist + 1, claim.Ocorhist);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithValidRequest_InsertsAccompaniment()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        _mockClaimAccompanimentRepository.Verify(r => r.AddAsync(
            It.Is<ClaimAccompaniment>(a => a.CodEvento == 1098),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithValidRequest_UpdatesPhases()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        _mockPhaseManagementService.Verify(p => p.UpdatePhasesAsync(
            claim.Fonte, claim.Protsini, claim.Dac, 1098,
            It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Validation Tests (Step 1)

    [Fact]
    public async Task AuthorizePaymentAsync_WithInvalidRequest_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Amount", "Amount must be greater than zero")
        });

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PaymentAuthorizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.False(result.Sucesso);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Amount must be greater than zero", result.Errors);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Amount", "Amount must be greater than zero"),
            new ValidationFailure("BeneficiaryName", "Beneficiary name is required"),
            new ValidationFailure("PaymentType", "Invalid payment type")
        });

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PaymentAuthorizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.Equal(3, result.Errors.Count);
    }

    #endregion

    #region Claim Search Tests (Step 2)

    [Fact]
    public async Task AuthorizePaymentAsync_WithNonExistentClaim_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();

        SetupSuccessfulValidation();
        _mockClaimRepository.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<ClaimMaster, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimMaster>());

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.False(result.Sucesso);
        Assert.Contains("não encontrado", result.Errors.First());
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithSystemControlNotFound_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        _mockSystemControlRepository.Setup(r => r.GetByIdAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SystemControl?)null);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.Contains("TSISTEMA", result.Errors.First());
    }

    #endregion

    #region External Validation Tests (Step 3)

    [Fact]
    public async Task AuthorizePaymentAsync_WithExternalValidationRequired_CallsRouter()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        request.ExternalValidationRequired = true;
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulExternalValidation();
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        _mockExternalServiceRouter.Verify(r => r.RouteAndValidateAsync(
            It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithExternalValidationFailure_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        request.ExternalValidationRequired = true;
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);

        _mockExternalServiceRouter.Setup(r => r.RouteAndValidateAsync(
                It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalValidationResponse
            {
                Ezert8 = "EZERT8001", // Non-success code
                ErrorMessage = "External validation failed"
            });

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.False(result.Sucesso);
        Assert.Contains("Validação externa falhou", result.Errors.First());
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithBypassValidation_SkipsExternalValidation()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        request.ExternalValidationRequired = true;
        request.BypassValidation = true;
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        _mockExternalServiceRouter.Verify(r => r.RouteAndValidateAsync(
            It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal("APPROVED", result.Status);
    }

    #endregion

    #region Business Rules Tests (Step 4)

    [Fact]
    public async Task AuthorizePaymentAsync_BR004_InvalidPaymentType_ReturnsRejected()
    {
        // Arrange - BR-004: Payment type must be 1-5
        var request = CreateValidPaymentRequest();
        request.PaymentType = 6; // Invalid
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.Contains("BR-004", result.Errors.First());
    }

    [Fact]
    public async Task AuthorizePaymentAsync_BR005_AmountExceedsPending_ReturnsRejected()
    {
        // Arrange - BR-005: Amount must not exceed pending balance
        var request = CreateValidPaymentRequest();
        request.Amount = 15000.00m; // Exceeds pending (10000 - 2500 = 7500)
        var claim = CreateValidClaim();
        claim.Sdopag = 10000.00m;
        claim.Totpag = 2500.00m;
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.Contains("BR-005", result.Errors.First());
        Assert.Contains("excede saldo pendente", result.Errors.First());
    }

    [Fact]
    public async Task AuthorizePaymentAsync_BR019_MissingBeneficiary_ReturnsRejected()
    {
        // Arrange - BR-019: Beneficiary required if TPSEGU != 0
        var request = CreateValidPaymentRequest();
        request.BeneficiaryName = string.Empty;
        var claim = CreateValidClaim();
        claim.Tpsegu = 1; // Not zero, beneficiary required
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.Contains("BR-019", result.Errors.First());
        Assert.Contains("Beneficiário obrigatório", result.Errors.First());
    }

    [Fact]
    public async Task AuthorizePaymentAsync_BR006_InvalidPolicyType_ReturnsRejected()
    {
        // Arrange - BR-006: Policy type must be '1' or '2'
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        claim.Tipreg = "3"; // Invalid
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.Contains("BR-006", result.Errors.First());
    }

    #endregion

    #region Currency Conversion Tests (BR-023)

    [Fact]
    public async Task AuthorizePaymentAsync_BR023_PerformsCurrencyConversion()
    {
        // Arrange - BR-023: Currency conversion VALPRIBT = VALPRI × VLCRUZAD
        var request = CreateValidPaymentRequest();
        request.Amount = 1000.00m;
        request.CurrencyCode = "BRL";
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        _mockCurrencyConversionService.Verify(c => c.ConvertAsync(
            request.Amount, request.CurrencyCode, "BTNF", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithCurrencyConversionFailure_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);

        _mockCurrencyConversionService.Setup(c => c.ConvertAsync(
                It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrencyConversionResult
            {
                IsSuccess = false,
                ErrorMessage = "Currency conversion failed"
            });

        SetupTransactionInfrastructure();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.True(result.RollbackOccurred);
    }

    #endregion

    #region Transaction Rollback Tests (BR-067)

    [Fact]
    public async Task AuthorizePaymentAsync_BR067_WithExceptionInTransaction_RollsBack()
    {
        // Arrange - BR-067: Transaction atomicity (all-or-nothing)
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupTransactionInfrastructure();

        // Simulate failure during SaveChanges
        var saveCount = 0;
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                saveCount++;
                if (saveCount == 2) // Fail on second save (after THISTSIN, during TMESTSIN update)
                    throw new InvalidOperationException("Database error");
                return Task.FromResult(1);
            });

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
        Assert.True(result.RollbackOccurred);
        Assert.NotNull(result.FailedStep);
        _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithRollback_DoesNotPersistChanges()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        var systemControl = CreateSystemControl();
        var originalTotpag = claim.Totpag;

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupTransactionInfrastructure();

        _mockClaimAccompanimentRepository.Setup(r => r.AddAsync(
                It.IsAny<ClaimAccompaniment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Accompaniment insert failed"));

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.True(result.RollbackOccurred);
        _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task AuthorizePaymentAsync_WithZeroAmount_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        request.Amount = 0.00m;

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Amount", "Amount must be greater than zero")
        });

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PaymentAuthorizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithNegativeAmount_ReturnsRejected()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        request.Amount = -1000.00m;

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Amount", "Amount must be greater than zero")
        });

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PaymentAuthorizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("REJECTED", result.Status);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_WithMaximumPendingAmount_Succeeds()
    {
        // Arrange
        var request = CreateValidPaymentRequest();
        var claim = CreateValidClaim();
        claim.Sdopag = 10000.00m;
        claim.Totpag = 2500.00m;
        request.Amount = 7500.00m; // Exactly the pending amount
        var systemControl = CreateSystemControl();

        SetupSuccessfulValidation();
        SetupClaimRepository(claim);
        SetupSystemControl(systemControl);
        SetupSuccessfulCurrencyConversion();
        SetupSuccessfulTransaction();

        // Act
        var result = await _service.AuthorizePaymentAsync(request);

        // Assert
        Assert.Equal("APPROVED", result.Status);
        Assert.Equal(10000.00m, claim.Totpag); // Fully paid
    }

    #endregion

    #region Helper Methods

    private PaymentAuthorizationRequest CreateValidPaymentRequest()
    {
        return new PaymentAuthorizationRequest
        {
            ClaimId = 1,
            Tipseg = 1,
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 12345,
            Amount = 1000.00m,
            CurrencyCode = "BRL",
            PaymentType = 1,
            BeneficiaryName = "João da Silva",
            BeneficiaryTaxId = "12345678901",
            RequestedBy = "TESTUSER",
            ExternalValidationRequired = false
        };
    }

    private ClaimMaster CreateValidClaim()
    {
        return new ClaimMaster
        {
            Tipseg = 1,
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 12345,
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            Sdopag = 10000.00m,
            Totpag = 2500.00m,
            Ocorhist = 5,
            Tipreg = "1",
            Tpsegu = 1,
            Codprodu = 1001
        };
    }

    private SystemControl CreateSystemControl()
    {
        return new SystemControl
        {
            Idsistem = "SI",
            Dtmovabe = DateTime.Today
        };
    }

    private void SetupSuccessfulValidation()
    {
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PaymentAuthorizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupClaimRepository(ClaimMaster claim)
    {
        _mockClaimRepository.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<ClaimMaster, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimMaster> { claim });
    }

    private void SetupSystemControl(SystemControl systemControl)
    {
        _mockSystemControlRepository.Setup(r => r.GetByIdAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(systemControl);
    }

    private void SetupSuccessfulExternalValidation()
    {
        _mockExternalServiceRouter.Setup(r => r.RouteAndValidateAsync(
                It.IsAny<ExternalValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalValidationResponse
            {
                Ezert8 = "00000000", // Success code
                ValidationService = "CNOUA",
                ElapsedMilliseconds = 150
            });
    }

    private void SetupSuccessfulCurrencyConversion()
    {
        _mockCurrencyConversionService.Setup(c => c.ConvertAsync(
                It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrencyConversionResult
            {
                IsSuccess = true,
                OriginalAmount = 1000.00m,
                ConvertedAmount = 1000.00m,
                ExchangeRate = 1.0m,
                SourceCurrency = "BRL",
                TargetCurrency = "BTNF"
            });
    }

    private void SetupTransactionInfrastructure()
    {
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockUnitOfWork.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockClaimHistoryRepository.Setup(r => r.AddAsync(It.IsAny<ClaimHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimHistory h, CancellationToken ct) => h);

        _mockClaimAccompanimentRepository.Setup(r => r.AddAsync(It.IsAny<ClaimAccompaniment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimAccompaniment a, CancellationToken ct) => a);

        _mockPhaseManagementService.Setup(p => p.UpdatePhasesAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<DateTime>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupSuccessfulTransaction()
    {
        SetupTransactionInfrastructure();
    }

    #endregion
}
