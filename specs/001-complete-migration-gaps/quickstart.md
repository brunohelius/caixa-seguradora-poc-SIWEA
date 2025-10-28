# Quickstart Guide: Complete SIWEA Migration - Gaps Implementation

**Feature**: Complete 100% SIWEA Migration
**Branch**: 001-complete-migration-gaps
**Date**: 2025-10-27

## Prerequisites

### Required Software
- **.NET 9.0 SDK** (9.0.0 or later)
- **Node.js 20.x** (for React frontend)
- **SQL Server 2019+** or **Azure SQL Database**
- **Git** (for branch management)
- **Visual Studio Code** or **Visual Studio 2022**

### Recommended Tools
- **Postman** or **Thunder Client** (API testing)
- **SQL Server Management Studio** (database inspection)
- **Playwright Test for VSCode** (E2E test debugging)

### Environment Setup

1. **Clone Repository and Switch to Feature Branch**:
   ```bash
   cd "/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age"
   git checkout 001-complete-migration-gaps
   git pull origin 001-complete-migration-gaps
   ```

2. **Restore Backend Dependencies**:
   ```bash
   cd backend
   dotnet restore
   dotnet build
   ```

3. **Restore Frontend Dependencies**:
   ```bash
   cd ../frontend
   npm install
   ```

4. **Configure Database Connection**:
   ```bash
   # Set user secrets for database connection string
   cd ../backend/src/CaixaSeguradora.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:ClaimsDatabase" "Server=localhost;Database=SIWEA;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
   ```

5. **Verify External Service Configuration** (appsettings.Development.json):
   ```json
   {
     "ExternalServices": {
       "CNOUA": {
         "BaseUrl": "https://cnoua-service-test.caixaseguradora.com.br",
         "Timeout": 10,
         "RetryCount": 3,
         "CircuitBreakerThreshold": 5,
         "CircuitBreakerDuration": 30
       },
       "SIPUA": {
         "BaseUrl": "https://sipua-service-test.caixaseguradora.com.br",
         "Timeout": 10,
         "RetryCount": 3,
         "CircuitBreakerThreshold": 5,
         "CircuitBreakerDuration": 30
       },
       "SIMDA": {
         "BaseUrl": "https://simda-service-test.caixaseguradora.com.br",
         "Timeout": 10,
         "RetryCount": 3,
         "CircuitBreakerThreshold": 5,
         "CircuitBreakerDuration": 30
       }
     }
   }
   ```

## Running the Application

### Backend (.NET 9 API)

```bash
cd backend/src/CaixaSeguradora.Api
dotnet run

# API will start on:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
# - Swagger: https://localhost:5001/swagger
```

### Frontend (React 19 SPA)

```bash
cd frontend
npm run dev

# Frontend will start on:
# - http://localhost:3000
```

### Docker Compose (Full Stack)

```bash
# From repository root
docker-compose up --build

# Services will start on:
# - Backend API: https://localhost:5001
# - Frontend: http://localhost:3000
# - SQL Server: localhost:1433
```

## Development Workflow

### 1. Implementing External Service Clients (Priority P1)

**Goal**: Complete CNOUA, SIPUA, SIMDA validation clients with Polly resilience

**Files to Create**:
```
backend/src/CaixaSeguradora.Infrastructure/ExternalServices/
├── CnouaValidationClient.cs
├── SipuaValidationClient.cs
└── SimdaValidationClient.cs

backend/src/CaixaSeguradora.Core/Interfaces/
├── ICnouaValidationClient.cs
├── ISipuaValidationClient.cs
└── ISimdaValidationClient.cs
```

**Implementation Steps**:

a. **Create Interface** (Core layer):
```csharp
// Core/Interfaces/ICnouaValidationClient.cs
public interface ICnouaValidationClient {
  Task<ExternalValidationResponse> ValidateAsync(
    ExternalValidationRequest request,
    CancellationToken cancellationToken = default);
}
```

b. **Implement Client** (Infrastructure layer):
```csharp
// Infrastructure/ExternalServices/CnouaValidationClient.cs
public class CnouaValidationClient : ICnouaValidationClient {
  private readonly HttpClient _httpClient;
  private readonly ILogger<CnouaValidationClient> _logger;
  private readonly IAsyncPolicy<HttpResponseMessage> _resilencePolicy;

  public CnouaValidationClient(
    HttpClient httpClient,
    ILogger<CnouaValidationClient> logger) {
    _httpClient = httpClient;
    _logger = logger;
    _resilencePolicy = CreateResiliencePolicy();
  }

  private IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy() {
    var retry = Policy
      .Handle<HttpRequestException>()
      .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
      .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) => {
          _logger.LogWarning("CNOUA retry {RetryCount} after {Delay}s", retryCount, timespan.TotalSeconds);
        });

    var circuitBreaker = Policy
      .Handle<HttpRequestException>()
      .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
        onBreak: (result, duration) => {
          _logger.LogError("CNOUA circuit breaker opened for {Duration}s", duration.TotalSeconds);
        },
        onReset: () => {
          _logger.LogInformation("CNOUA circuit breaker closed");
        });

    var timeout = Policy.TimeoutAsync(10);

    return Policy.WrapAsync(retry, circuitBreaker, timeout);
  }

  public async Task<ExternalValidationResponse> ValidateAsync(
    ExternalValidationRequest request,
    CancellationToken cancellationToken) {
    var stopwatch = Stopwatch.StartNew();
    var requestTimestamp = DateTime.UtcNow;

    try {
      var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/validate") {
        Content = JsonContent.Create(request)
      };

      var response = await _resilencePolicy.ExecuteAsync(async () =>
        await _httpClient.SendAsync(httpRequest, cancellationToken));

      var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
      var validationResponse = JsonSerializer.Deserialize<CnouaResponse>(responseBody);

      stopwatch.Stop();

      return new ExternalValidationResponse {
        Ezert8 = validationResponse.Ezert8,
        ErrorMessage = validationResponse.Message,
        IsSuccess = validationResponse.Ezert8 == "00000000",
        ValidationService = "CNOUA",
        RequestTimestamp = requestTimestamp,
        ResponseTimestamp = DateTime.UtcNow,
        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
      };
    }
    catch (Exception ex) {
      _logger.LogError(ex, "CNOUA validation failed");
      stopwatch.Stop();

      return new ExternalValidationResponse {
        Ezert8 = "SYS-005",
        ErrorMessage = "Serviço de validação indisponível",
        IsSuccess = false,
        ValidationService = "CNOUA",
        RequestTimestamp = requestTimestamp,
        ResponseTimestamp = DateTime.UtcNow,
        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
      };
    }
  }
}
```

c. **Register in DI Container** (Program.cs):
```csharp
// Configure HttpClient with Polly policies
builder.Services.AddHttpClient<ICnouaValidationClient, CnouaValidationClient>(client => {
  client.BaseAddress = new Uri(builder.Configuration["ExternalServices:CNOUA:BaseUrl"]);
  client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<ISipuaValidationClient, SipuaValidationClient>(...);
builder.Services.AddHttpClient<ISimdaValidationClient, SimdaValidationClient>(...);
```

d. **Test Client**:
```bash
cd backend/tests/CaixaSeguradora.Infrastructure.Tests
dotnet test --filter "FullyQualifiedName~CnouaValidationClientTests"
```

### 2. Implementing Transaction Atomicity (Priority P1)

**Goal**: Complete 4-step transaction pipeline with rollback

**Files to Modify**:
```
backend/src/CaixaSeguradora.Infrastructure/Services/
└── PaymentAuthorizationService.cs (enhance existing)

backend/src/CaixaSeguradora.Infrastructure/Services/
└── TransactionCoordinator.cs (new)
```

**Implementation Steps**:

a. **Create TransactionCoordinator**:
```csharp
// Infrastructure/Services/TransactionCoordinator.cs
public class TransactionCoordinator {
  private readonly ClaimsDbContext _dbContext;
  private readonly ILogger<TransactionCoordinator> _logger;

  public async Task<PaymentAuthorizationResponse> ExecuteAtomicTransactionAsync(
    PaymentAuthorizationRequest request,
    Func<IDbContextTransaction, Task> transactionLogic,
    CancellationToken cancellationToken) {

    using var transaction = await _dbContext.Database.BeginTransactionAsync(
      IsolationLevel.ReadCommitted, cancellationToken);

    var context = new TransactionContext {
      AuthorizationId = Guid.NewGuid(),
      OperationCode = 1098,
      CorrectionType = "5",
      Step = TransactionStep.History
    };

    try {
      await transactionLogic(transaction);
      await transaction.CommitAsync(cancellationToken);

      _logger.LogInformation("Transaction committed: {AuthId}", context.AuthorizationId);

      return CreateSuccessResponse(context);
    }
    catch (Exception ex) {
      await transaction.RollbackAsync(cancellationToken);

      _logger.LogError(ex, "Transaction rollback at step: {Step}", context.Step);

      return CreateRollbackResponse(context, ex);
    }
  }
}
```

b. **Enhance PaymentAuthorizationService**:
```csharp
public async Task<PaymentAuthorizationResponse> AuthorizePaymentAsync(
  PaymentAuthorizationRequest request,
  CancellationToken cancellationToken) {

  // Step 1: Validate request
  var validationResult = await _validator.ValidateAsync(request, cancellationToken);
  if (!validationResult.IsValid) {
    return CreateValidationErrorResponse(validationResult.Errors);
  }

  // Step 2: External validation
  if (request.ExternalValidationRequired) {
    var validationResponse = await CallExternalService(request, cancellationToken);
    if (!validationResponse.IsSuccess) {
      return CreateExternalValidationErrorResponse(validationResponse);
    }
  }

  // Step 3: Execute atomic transaction
  return await _transactionCoordinator.ExecuteAtomicTransactionAsync(
    request,
    async (transaction) => {
      // Step 3a: Insert THISTSIN
      var history = CreateHistoryRecord(request);
      await _unitOfWork.ClaimHistories.AddAsync(history);
      await _unitOfWork.SaveChangesAsync();

      // Step 3b: Update TMESTSIN
      var claim = await _unitOfWork.Claims.GetByIdAsync(request.ClaimKey);
      claim.Totpag += request.ValorPrincipal;
      claim.Ocorhist++;
      await _unitOfWork.SaveChangesAsync();

      // Step 3c: Insert SI_ACOMPANHA_SINI
      var accompaniment = CreateAccompanimentEvent(request);
      await _unitOfWork.ClaimAccompaniments.AddAsync(accompaniment);
      await _unitOfWork.SaveChangesAsync();

      // Step 3d: Update SI_SINISTRO_FASE
      await _phaseManagementService.ProcessPhaseChangesAsync(request, transaction);
      await _unitOfWork.SaveChangesAsync();
    },
    cancellationToken);
}
```

c. **Test Transaction Rollback**:
```bash
cd backend/tests/CaixaSeguradora.Api.Tests/Integration
dotnet test --filter "FullyQualifiedName~TransactionRollbackTests"
```

### 3. Implementing Business Rules Validators (Priority P2)

**Goal**: Enforce all 100+ business rules with FluentValidation

**Files to Create**:
```
backend/src/CaixaSeguradora.Core/Validators/BusinessRules/
├── SearchValidationRules.cs (BR-001 to BR-009)
├── PaymentValidationRules.cs (BR-010 to BR-026)
├── CurrencyConversionRules.cs (BR-027 to BR-037)
├── TransactionRecordingRules.cs (BR-038 to BR-046)
├── ProductValidationRules.cs (BR-047 to BR-060)
├── PhaseManagementRules.cs (BR-061 to BR-070)
├── AuditTrailRules.cs (BR-071 to BR-074)
├── DataValidationRules.cs (BR-075 to BR-087)
├── UiDisplayRules.cs (BR-088 to BR-095)
└── PerformanceRules.cs (BR-096 to BR-099)
```

**Implementation Example** (BR-019: Beneficiary Required):
```csharp
// Core/Validators/BusinessRules/PaymentValidationRules.cs
public class PaymentValidationRules : AbstractValidator<PaymentAuthorizationRequest> {
  public PaymentValidationRules() {
    // BR-019: Beneficiary required when TPSEGU != 0
    RuleFor(x => x.Beneficiario)
      .NotEmpty()
      .When(x => x.TipoSeguro != 0)
      .WithMessage(ErrorMessages.VAL_007) // "Favorecido é obrigatório para este tipo de seguro"
      .WithErrorCode("BR-019");

    // BR-013: Amount <= pending value
    RuleFor(x => x.ValorPrincipal)
      .LessThanOrEqualTo(x => x.SaldoPendente)
      .WithMessage(ErrorMessages.VAL_005) // "Valor Superior ao Saldo Pendente"
      .WithErrorCode("BR-013");
  }
}
```

**Test Each Rule**:
```bash
cd backend/tests/CaixaSeguradora.Core.Tests/BusinessRules
dotnet test --filter "FullyQualifiedName~BR019_BeneficiaryRequirement"
```

## Testing

### Run All Unit Tests
```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

### Run Integration Tests
```bash
cd backend/tests/CaixaSeguradora.Api.Tests
dotnet test --filter "Category=Integration"
```

### Run E2E Tests (Playwright)
```bash
cd frontend
npx playwright test

# Run with UI mode for debugging
npx playwright test --ui
```

### Run Performance Tests
```bash
cd backend/tests/CaixaSeguradora.Api.Tests
dotnet test --filter "Category=Performance"

# Should verify:
# - Search < 3 seconds
# - Authorization < 90 seconds
# - History query < 500ms
```

## Debugging

### Backend API Debugging (VS Code)

1. Open `.vscode/launch.json` and add:
```json
{
  "name": ".NET Core Launch (API)",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build",
  "program": "${workspaceFolder}/backend/src/CaixaSeguradora.Api/bin/Debug/net9.0/CaixaSeguradora.Api.dll",
  "args": [],
  "cwd": "${workspaceFolder}/backend/src/CaixaSeguradora.Api",
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

2. Set breakpoints in PaymentAuthorizationService.cs
3. Press F5 to start debugging

### Frontend React Debugging (Chrome DevTools)

1. Run `npm run dev`
2. Open Chrome DevTools (F12)
3. Go to Sources tab → Filesystem → Add folder to workspace
4. Set breakpoints in TypeScript files

### External Service Stubbing (WireMock)

1. Install WireMock:
```bash
npm install -g wiremock
```

2. Start WireMock server:
```bash
wiremock --port 8080 --verbose
```

3. Create stub for CNOUA:
```bash
curl -X POST http://localhost:8080/__admin/mappings \
  -H "Content-Type: application/json" \
  -d '{
    "request": {
      "method": "POST",
      "url": "/api/v1/validate"
    },
    "response": {
      "status": 200,
      "body": "{\"ezert8\":\"00000000\",\"message\":\"Validation successful\"}",
      "headers": {
        "Content-Type": "application/json"
      }
    }
  }'
```

4. Update appsettings.Development.json to point to WireMock:
```json
{
  "ExternalServices": {
    "CNOUA": {
      "BaseUrl": "http://localhost:8080"
    }
  }
}
```

## Common Issues and Solutions

### Issue 1: External Service Timeout

**Symptom**: SYS-005 error "Serviço de validação indisponível"

**Solution**:
- Check external service endpoint in appsettings.Development.json
- Verify network connectivity to external service
- Check circuit breaker status in logs (may be open after 5 failures)
- Use WireMock stub for local development

### Issue 2: Transaction Rollback on Phase Update

**Symptom**: SYS-004 error "Erro ao processar fases"

**Solution**:
- Verify SI_REL_FASE_EVENTO has configuration for event code 1098
- Check database constraints on SI_SINISTRO_FASE table
- Verify DATA_FECHA_SIFA format (must be '9999-12-31' for open phases)
- Review transaction logs for specific SQL error

### Issue 3: Currency Conversion Rate Not Found

**Symptom**: VAL-008 error "Taxa de conversão não disponível para a data"

**Solution**:
- Verify TGEUNIMO table has rate for current DTMOVABE
- Check date range (DTINIVIG <= DATE <= DTTERVIG)
- Seed test data: `INSERT INTO TGEUNIMO (DTINIVIG, DTTERVIG, VLCRUZAD) VALUES ('2025-01-01', '2025-12-31', 1.23456789)`

### Issue 4: Beneficiary Validation Failing

**Symptom**: VAL-007 error when TPSEGU = 0

**Solution**:
- Verify TPSEGU value in claim master record
- Check PaymentValidationRules.cs condition: `When(x => x.TipoSeguro != 0)`
- Review request payload to ensure TipoSeguro field is populated

## Next Steps

1. **Complete P1 Implementation** (External Services + Transaction Atomicity)
   - Run: `dotnet test --filter "Priority=P1"`
   - Verify all P1 tests pass before proceeding

2. **Complete P2 Implementation** (Business Rules Enforcement)
   - Run: `dotnet test --filter "Priority=P2"`
   - Verify all 100+ business rules have passing tests

3. **Complete P3 Implementation** (UI/Display + Testing)
   - Run: `npx playwright test`
   - Verify E2E tests pass for complete user journeys

4. **Performance Tuning** (Database Indexes + Query Optimization)
   - Run: `dotnet test --filter "Category=Performance"`
   - Verify performance targets met (< 3s search, < 90s authorization, < 500ms history)

5. **Final Validation** (100% Migration Complete)
   - Run: `dotnet test`
   - Run: `npx playwright test`
   - Verify all 10 success criteria (SC-001 through SC-010) met

---

**Quickstart Status**: COMPLETE
**Ready for Development**: YES
**Next Command**: `/speckit.tasks` to generate implementation tasks
