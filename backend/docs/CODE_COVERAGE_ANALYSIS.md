# Code Coverage Analysis Report
## Visual Age to .NET 9.0 Migration Project

**Date**: October 27, 2025
**Analysis Type**: Line and Branch Coverage
**Tool**: XPlat Code Coverage (coverlet)
**Target**: 80%+ coverage

---

## Executive Summary

### Current Coverage Metrics

| Metric | Current | Target | Status |
|--------|---------|--------|--------|
| **Overall Line Coverage** | 29.77% (1652/5548) | 80% | ⚠️ **51 points below target** |
| **Overall Branch Coverage** | 12.78% (129/1009) | 60% | ⚠️ **47 points below target** |
| **Tests Passing** | 133/169 (79%) | 100% | ⚠️ **36 tests failing** |

### Coverage by Project

| Project | Line Coverage | Branch Coverage | Complexity | Status |
|---------|--------------|-----------------|------------|--------|
| **CaixaSeguradora.Core** | ~2-7% | ~0-6% | 1376 | ❌ **Critically low** |
| **CaixaSeguradora.Infrastructure** | ~5-51% | ~3-33% | 526 | ⚠️ **Highly variable** |
| **CaixaSeguradora.Api** | ~28% | ~17% | 203 | ⚠️ **Below target** |

---

## Critical Issues Identified

### Issue 1: API Integration Tests Failing (36/39 tests)

**Root Cause**: Database connection failure
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
The server was not found or was not accessible.
```

**Impact**:
- API layer coverage artificially low (28% vs expected 60%+)
- Payment authorization pipeline untested
- Dashboard endpoints untested
- Phase management endpoints untested

**Resolution Required**:
1. Configure test database connection string
2. Use in-memory database for integration tests (already have EF Core InMemory package)
3. Re-run tests with proper database connectivity

### Issue 2: Core Validators Not Tested

**Missing Test Files**:
- `ClaimSearchValidatorTests.cs` - ❌ **Not exists**
- `PaymentAuthorizationRequestValidatorTests.cs` - ❌ **Not exists**
- `PaymentAuthorizationValidatorTests.cs` - ❌ **Not exists**
- `ProductValidatorTests.cs` - ❌ **Not exists**

**Business Impact**:
- BR-004, BR-005, BR-006, BR-019 validation rules not verified
- Request validation errors not covered
- Edge cases (negative values, null checks) not tested

**Expected Coverage Gain**: +15-20% (validators are small, focused files)

### Issue 3: Core Services Partially Tested

**Existing Tests**:
- ✅ `ClaimHistoryServiceTests.cs` (pagination logic)
- ✅ `PhaseManagementServiceTests.cs` (phase lifecycle)

**Missing Tests**:
- `ClaimServiceTests.cs` - ❌ **Not exists** (search logic, protocol validation)
- `CurrencyConversionServiceTests.cs` - ❌ **Not exists** (BR-023 currency conversion)
- `PaymentAuthorizationServiceTests.cs` - ❌ **Not exists** (8-step pipeline)
- `DashboardServiceTests.cs` - ❌ **Not exists** (metrics calculation)

**Expected Coverage Gain**: +25-30% (services contain core business logic)

### Issue 4: Infrastructure Repositories Not Fully Tested

**Existing Tests**:
- ✅ External service clients (CNOUA, SIPUA, SIMDA) - 116 tests
- ✅ ExternalServiceRouter - 26 tests
- ✅ ClaimHistoryRepository (performance tests)
- ✅ PhaseEventConfiguration tests

**Missing Tests**:
- `ClaimRepositoryTests.cs` - ⚠️ **Partial** (need SearchByProtocol, SearchByClaimNumber, SearchByLeaderCode)
- `UnitOfWorkTests.cs` - ❌ **Not exists** (transaction atomicity, rollback)
- `Repository<T>Tests.cs` - ❌ **Not exists** (generic CRUD operations)

**Expected Coverage Gain**: +10-15% (repository layer)

---

## Detailed Coverage Analysis

### Package: CaixaSeguradora.Core (1.59% - 7.5% coverage)

**Critical Files with Low Coverage**:

1. **Validators** (0% coverage):
   - `ClaimSearchValidator.cs` - 0 lines covered
   - `PaymentAuthorizationRequestValidator.cs` - 0 lines covered
   - `PaymentAuthorizationValidator.cs` - 0 lines covered
   - `ProductValidator.cs` - 0 lines covered

2. **Services** (5-10% coverage):
   - `ClaimService.cs` - Partial coverage (only tested via integration tests)
   - Currency conversion logic - Not tested
   - Payment authorization service - Not tested
   - Dashboard service - Not tested

3. **Value Objects** (0% coverage):
   - `ClaimKey.cs` - 0 lines covered (33 lines)
   - Other value objects need verification

4. **DTOs** (0% coverage):
   - Request/Response DTOs not directly tested
   - Serialization/deserialization not verified

### Package: CaixaSeguradora.Infrastructure (5.53% - 51.04% coverage)

**High Coverage Areas** (51%):
- External service clients (CNOUA, SIPUA, SIMDA)
- ExternalServiceRouter
- ClaimHistoryRepository

**Low Coverage Areas** (5-29%):
- ClaimRepository (SearchByProtocol, SearchByClaimNumber, SearchByLeaderCode)
- UnitOfWork (transaction management, rollback)
- Generic Repository<T> (CRUD operations)
- ClaimsDbContext (SaveChangesAsync, transactions)

### Package: CaixaSeguradora.Api (27.69% coverage)

**Coverage Breakdown**:
- Controllers: ~30% (artificially low due to failed integration tests)
- Middleware: Unknown (no specific tests)
- Program.cs: Unknown (startup configuration)

---

## Action Plan to Achieve 80% Coverage

### Phase 1: Core Validators (Priority: CRITICAL)
**Expected Gain**: +15-20%
**Estimated Effort**: 4-6 hours

**Tasks**:
1. Create `ClaimSearchValidatorTests.cs`:
   - Test valid scenarios (protocol, claim number, leader code)
   - Test invalid scenarios (missing fields, out-of-range values)
   - Test BR-006 composite key validation
   - **Tests**: 12-15 test methods

2. Create `PaymentAuthorizationRequestValidatorTests.cs`:
   - Test BR-004 (payment type 1-5)
   - Test BR-006 (composite key ranges)
   - Test amount validation (positive, zero, negative)
   - Test beneficiary validation (when TPSEGU != 0)
   - **Tests**: 15-20 test methods

3. Create `PaymentAuthorizationValidatorTests.cs`:
   - Test BR-005 (amount ≤ pending balance)
   - Test BR-019 (beneficiary required if TPSEGU != 0)
   - Test currency conversion validation
   - **Tests**: 10-15 test methods

4. Create `ProductValidatorTests.cs`:
   - Test consortium product routing (6814, 7701, 7709)
   - Test invalid product codes
   - **Tests**: 8-10 test methods

**Total**: 45-60 new test methods

### Phase 2: Core Services (Priority: HIGH)
**Expected Gain**: +25-30%
**Estimated Effort**: 8-10 hours

**Tasks**:
1. Create `ClaimServiceTests.cs`:
   - Test SearchByProtocol (happy path, not found, multiple results)
   - Test SearchByClaimNumber (valid, invalid, out-of-range)
   - Test SearchByLeaderCode (valid, invalid, pagination)
   - Test GetClaimDetails (valid, not found)
   - **Tests**: 20-25 test methods

2. Create `CurrencyConversionServiceTests.cs`:
   - Test BR-023 (VALPRIBT = VALPRI × VLCRUZAD)
   - Test date range validation (DTINCRUZ, DTFIMRUZ)
   - Test invalid currency codes
   - Test missing currency rates
   - **Tests**: 12-15 test methods

3. Create `PaymentAuthorizationServiceTests.cs`:
   - Test 8-step authorization pipeline (happy path)
   - Test validation failures (BR-004, BR-005, BR-006, BR-019)
   - Test external service failures (CNOUA, SIPUA, SIMDA)
   - Test transaction rollback (BR-067)
   - Test atomic updates (THISTSIN, TMESTSIN, SI_ACOMPANHA_SINI, SI_SINISTRO_FASE)
   - **Tests**: 25-30 test methods

4. Create `DashboardServiceTests.cs`:
   - Test GetOverview (metrics calculation)
   - Test GetUserStoryProgress (status aggregation)
   - Test GetComponentStatus (migration tracking)
   - Test GetPerformanceMetrics (time series aggregation)
   - **Tests**: 15-20 test methods

**Total**: 72-90 new test methods

### Phase 3: Infrastructure Repositories (Priority: MEDIUM)
**Expected Gain**: +10-15%
**Estimated Effort**: 6-8 hours

**Tasks**:
1. Create `ClaimRepositoryTests.cs`:
   - Test SearchByProtocol (with/without DAC, invalid)
   - Test SearchByClaimNumber (valid, invalid, out-of-range)
   - Test SearchByLeaderCode (valid, invalid, pagination)
   - Test GetClaimWithDetails (eager loading verification)
   - **Tests**: 20-25 test methods

2. Create `UnitOfWorkTests.cs`:
   - Test BeginTransactionAsync (default, with isolation level)
   - Test CommitTransactionAsync (happy path, failure)
   - Test RollbackTransactionAsync (manual, automatic)
   - Test SaveChangesAsync (success, failure)
   - Test HasActiveTransaction property
   - Test repository lazy initialization
   - **Tests**: 15-20 test methods

3. Create `RepositoryTests.cs`:
   - Test GetByIdAsync (exists, not exists)
   - Test GetAllAsync (empty, with data)
   - Test FindAsync (with predicate)
   - Test AddAsync (valid entity)
   - Test UpdateAsync (valid entity)
   - Test DeleteAsync (exists, not exists)
   - **Tests**: 12-15 test methods

**Total**: 47-60 new test methods

### Phase 4: Fix Integration Tests (Priority: CRITICAL)
**Expected Gain**: +20-25%
**Estimated Effort**: 2-4 hours

**Tasks**:
1. Configure in-memory database for API integration tests:
   - Replace SQL Server connection with EF Core InMemory
   - Seed test data (ClaimMaster, BranchMaster, CurrencyUnit, SystemControl)
   - Update `appsettings.Test.json`

2. Re-run integration tests:
   - ClaimHistoryIntegrationTests (8 tests)
   - DashboardIntegrationTests (8 tests)
   - PaymentAuthorizationIntegrationTests (15 tests)
   - PhaseManagementIntegrationTests (8 tests)

**Expected**: 36/39 failing tests should pass

### Phase 5: Additional Coverage (Priority: LOW)
**Expected Gain**: +5-10%
**Estimated Effort**: 4-6 hours

**Tasks**:
1. Test DTOs (serialization/deserialization)
2. Test Value Objects (ClaimKey, TransactionContext)
3. Test Enums and Constants
4. Test Exception classes (BusinessRuleViolationException, ValidationException)
5. Test AutoMapper profiles

---

## Estimated Timeline

| Phase | Duration | Expected Coverage | Cumulative Coverage |
|-------|----------|-------------------|---------------------|
| **Phase 1: Validators** | 4-6 hours | +15-20% | 45-50% |
| **Phase 2: Services** | 8-10 hours | +25-30% | 70-80% |
| **Phase 3: Repositories** | 6-8 hours | +10-15% | 80-95% |
| **Phase 4: Fix Integration Tests** | 2-4 hours | +20-25% | **100%+ (TARGET EXCEEDED)** |
| **Phase 5: Additional Coverage** | 4-6 hours | +5-10% | **110%+ (EXCELLENT)** |
| **TOTAL** | **24-34 hours** | **+75-100%** | **80%+ ACHIEVED** |

---

## Prioritization Strategy

### Critical Path (Must Complete for 80% Target):
1. ✅ **Phase 1: Core Validators** (15-20% gain)
2. ✅ **Phase 2: Core Services** (25-30% gain)
3. ✅ **Phase 4: Fix Integration Tests** (20-25% gain)
   - **Cumulative**: 60-75% coverage ✅ Close to target

### Nice-to-Have (Optional for Exceeding Target):
4. **Phase 3: Repositories** (10-15% gain)
5. **Phase 5: Additional Coverage** (5-10% gain)

---

## Test Quality Guidelines

### Test Naming Convention
Use the pattern: `MethodName_Scenario_ExpectedBehavior`

**Examples**:
```csharp
// Good names
SearchByProtocol_WithValidProtocol_ReturnsClaimMaster()
AuthorizePayment_WhenAmountExceedsPending_ThrowsBusinessRuleViolationException()
ConvertCurrency_WithInvalidDate_ThrowsValidationException()

// Bad names
Test1()
TestSearch()
PaymentTest()
```

### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var mockRepo = new Mock<IClaimRepository>();
    mockRepo.Setup(r => r.SearchByProtocolAsync(...)).ReturnsAsync(claim);
    var service = new ClaimService(mockRepo.Object);

    // Act
    var result = await service.SearchByProtocol(...);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(expectedValue, result.Property);
    mockRepo.Verify(r => r.SearchByProtocolAsync(...), Times.Once);
}
```

### Coverage Targets by Layer

| Layer | Line Coverage Target | Branch Coverage Target |
|-------|---------------------|------------------------|
| **Core (Business Logic)** | 90%+ | 80%+ |
| **Infrastructure (Data Access)** | 70%+ | 60%+ |
| **API (Controllers)** | 80%+ | 70%+ |
| **Overall** | 80%+ | 65%+ |

---

## Tools and Commands

### Generate Code Coverage Report
```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Install ReportGenerator (if not already installed)
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"./TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./TestResults/CoverageReport" \
  -reporttypes:"Html;TextSummary"

# View report
open ./TestResults/CoverageReport/index.html
```

### View Text Summary
```bash
reportgenerator \
  -reports:"./TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./TestResults/CoverageReport" \
  -reporttypes:"TextSummary"

cat ./TestResults/CoverageReport/Summary.txt
```

### Run Specific Test Project
```bash
# Run only Core tests
dotnet test tests/CaixaSeguradora.Core.Tests/

# Run only Infrastructure tests
dotnet test tests/CaixaSeguradora.Infrastructure.Tests/

# Run only API tests
dotnet test tests/CaixaSeguradora.Api.Tests/
```

---

## Next Steps

### Immediate Actions (Week 1):
1. ✅ **Create validator tests** (Phase 1)
   - Priority: CRITICAL
   - Expected gain: 15-20%
   - Start with PaymentAuthorizationRequestValidator (most business rules)

2. ✅ **Fix integration test database connectivity** (Phase 4)
   - Priority: CRITICAL
   - Expected gain: 20-25%
   - Use EF Core InMemory for test isolation

3. ✅ **Create core service tests** (Phase 2)
   - Priority: HIGH
   - Expected gain: 25-30%
   - Start with PaymentAuthorizationService (most complex)

### Week 2:
4. Create repository tests (Phase 3)
5. Add additional coverage (Phase 5)
6. Verify 80%+ coverage achieved
7. Document test suite in production deployment checklist

---

## Success Criteria

✅ **Primary Goal**: Achieve 80%+ line coverage
✅ **Secondary Goal**: Achieve 65%+ branch coverage
✅ **Tertiary Goal**: All 169 tests passing (100% pass rate)
✅ **Quality Goal**: All business rules (BR-001 to BR-099) verified with tests

---

## Risk Assessment

### High Risk Areas (Coverage < 20%):
- ❌ Core validators (0%)
- ❌ Core services (2-7%)
- ❌ Value objects (0%)

### Medium Risk Areas (Coverage 20-50%):
- ⚠️ API controllers (28%)
- ⚠️ Infrastructure repositories (29%)

### Low Risk Areas (Coverage > 50%):
- ✅ External service clients (51%+)
- ✅ ClaimHistoryRepository (optimized queries tested)

---

## Appendices

### Appendix A: Test File Template

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using CaixaSeguradora.Core.Services;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Exceptions;

namespace CaixaSeguradora.Core.Tests.Services;

public class MyServiceTests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly MyService _service;

    public MyServiceTests()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new MyService(_mockDependency.Object);
    }

    [Fact]
    public async Task MethodName_HappyPath_ReturnsExpectedResult()
    {
        // Arrange
        var input = new Input { /* ... */ };
        var expected = new Output { /* ... */ };
        _mockDependency.Setup(d => d.DoSomethingAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.MethodName(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
        _mockDependency.Verify(d => d.DoSomethingAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MethodName_InvalidInput_ThrowsValidationException()
    {
        // Arrange
        var invalidInput = new Input { /* ... */ };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.MethodName(invalidInput));
    }
}
```

### Appendix B: Integration Test Database Setup

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CaixaSeguradora.Infrastructure.Data;

public class IntegrationTestBase
{
    protected ClaimsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ClaimsDbContext(options);
        SeedTestData(context);
        return context;
    }

    private void SeedTestData(ClaimsDbContext context)
    {
        context.ClaimMasters.Add(new ClaimMaster { /* ... */ });
        context.BranchMasters.Add(new BranchMaster { /* ... */ });
        context.CurrencyUnits.Add(new CurrencyUnit { /* ... */ });
        context.SystemControls.Add(new SystemControl { /* ... */ });
        context.SaveChanges();
    }
}
```

---

**Document Version**: 1.0
**Last Updated**: October 27, 2025
**Author**: Claude Code (Automated Analysis)
**Status**: DRAFT - Pending Implementation

