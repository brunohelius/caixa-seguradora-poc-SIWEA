# Diagramas de Classes - SIWEA

## Diagrama de Classes Principal - Domain Model

```mermaid
classDiagram
    %% ENTIDADES PRINCIPAIS
    class ClaimMaster {
        <<Entity>>
        +int InsuranceType
        +int ClaimOrigin
        +int ClaimBranch
        +int ClaimNumber
        +int Source
        +int ProtocolNumber
        +int DAC
        +int PolicyOrigin
        +int PolicyBranch
        +int PolicyNumber
        +int ProductCode
        +decimal ExpectedReserve
        +decimal TotalPaid
        +int OccurrenceNumber
        +string BeneficiaryName
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +decimal PendingValue()
        +bool CanAuthorizePayment(decimal amount)
        +void IncrementOccurrence()
    }

    class ClaimHistory {
        <<Entity>>
        +int InsuranceType
        +int ClaimOrigin
        +int ClaimBranch
        +int ClaimNumber
        +int OccurrenceNumber
        +int OperationCode
        +DateTime TransactionDate
        +TimeSpan OperationTime
        +decimal PrincipalValue
        +decimal CorrectionValue
        +string BeneficiaryName
        +string CorrectionType
        +decimal PrincipalValueBTNF
        +decimal CorrectionValueBTNF
        +decimal TotalValueBTNF
        +string AccountingStatus
        +string Status
        +string OperatorId
        +DateTime CreatedAt
        +decimal CalculateTotalBTNF()
    }

    class BranchMaster {
        <<Entity>>
        +int BranchCode
        +string BranchName
        +string Description
        +bool IsActive
        +DateTime CreatedAt
        +DateTime UpdatedAt
    }

    class PolicyMaster {
        <<Entity>>
        +int PolicyOrigin
        +int PolicyBranch
        +int PolicyNumber
        +string InsuredName
        +string CPF_CNPJ
        +int InsuranceType
        +DateTime PolicyStartDate
        +DateTime PolicyEndDate
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsActive()
    }

    class CurrencyUnit {
        <<Entity>>
        +DateTime StartDate
        +DateTime EndDate
        +decimal ConversionRate
        +string CurrencyCode
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsValidForDate(DateTime date)
        +decimal Convert(decimal value)
    }

    class SystemControl {
        <<Entity>>
        +string SystemId
        +DateTime CurrentBusinessDate
        +DateTime LastUpdate
        +string Status
        +DateTime GetBusinessDate()
    }

    class ClaimAccompaniment {
        <<Entity>>
        +int Source
        +int ProtocolNumber
        +int DAC
        +int EventCode
        +DateTime MovementDate
        +int OccurrenceNumber
        +string ComplementaryDescription
        +string UserId
        +DateTime CreatedAt
    }

    class ClaimPhase {
        <<Entity>>
        +int Source
        +int ProtocolNumber
        +int DAC
        +int PhaseCode
        +int EventCode
        +DateTime OpeningDate
        +DateTime ClosingDate
        +string Status
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsOpen()
        +void Close(DateTime date)
    }

    class PhaseEventRelationship {
        <<Entity>>
        +int EventCode
        +int PhaseCode
        +string AlterationType
        +DateTime EffectiveStartDate
        +DateTime EffectiveEndDate
        +DateTime CreatedAt
        +bool IsOpenAction()
        +bool IsCloseAction()
    }

    class ConsortiumContract {
        <<Entity>>
        +int ContractNumber
        +int PolicyOrigin
        +int PolicyBranch
        +int PolicyNumber
        +string ContractType
        +DateTime StartDate
        +DateTime EndDate
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsEFPContract()
        +bool IsHBContract()
    }

    %% RELACIONAMENTOS
    ClaimMaster "1" --> "0..*" ClaimHistory : has history
    ClaimMaster "1" --> "0..*" ClaimAccompaniment : has tracking
    ClaimMaster "1" --> "0..*" ClaimPhase : has phases
    ClaimMaster "*" --> "1" BranchMaster : belongs to branch
    ClaimMaster "*" --> "1" PolicyMaster : belongs to policy
    PolicyMaster "*" --> "0..1" ConsortiumContract : may have contract
    ClaimPhase "*" --> "1" PhaseEventRelationship : configured by

    %% VALUE OBJECTS
    class PaymentAuthorization {
        <<Value Object>>
        +int PaymentType
        +int PolicyType
        +decimal PrincipalValue
        +decimal CorrectionValue
        +string BeneficiaryName
        +DateTime TransactionDate
        +void Validate()
        +bool IsValid()
    }

    class ConvertedValues {
        <<Value Object>>
        +decimal PrincipalValueBTNF
        +decimal CorrectionValueBTNF
        +decimal TotalValueBTNF
        +decimal ConversionRate
        +DateTime ConversionDate
        +decimal GetTotalBTNF()
    }

    class ValidationResult {
        <<Value Object>>
        +string EZERT8Code
        +bool IsApproved
        +string Message
        +string ValidationId
        +DateTime ValidationTimestamp
        +bool IsSuccess()
    }

    %% RELACIONAMENTOS COM VALUE OBJECTS
    ClaimHistory --> ConvertedValues : contains
    PaymentAuthorization --> ConvertedValues : converts to
```

---

## Diagrama de Classes - Camada de Domínio (Core)

```mermaid
classDiagram
    %% DOMAIN SERVICES
    class PaymentAuthorizationService {
        <<Domain Service>>
        -IClaimRepository claimRepository
        -ICurrencyConverter currencyConverter
        -IProductValidator productValidator
        -IPhaseManager phaseManager
        +AuthorizePayment(claimId, authorization) Result
        +ValidateAuthorization(authorization) bool
        -CalculateBTNFValues(authorization) ConvertedValues
        -ValidateProduct(productCode, claimData) ValidationResult
        -RecordTransaction(claim, authorization, convertedValues) void
    }

    class CurrencyConversionService {
        <<Domain Service>>
        -ICurrencyUnitRepository currencyRepository
        -ISystemControlRepository systemRepository
        +ConvertToBTNF(value, date) decimal
        +GetCurrentRate(date) CurrencyUnit
        +ValidateRateAvailability(date) bool
        -ApplyBankersRounding(value) decimal
    }

    class ProductValidationService {
        <<Domain Service>>
        -ICNOUAClient cnouaClient
        -ISIPUAClient sipuaClient
        -ISIMDAClient simdaClient
        -IConsortiumContractRepository contractRepository
        +ValidateProduct(productCode, claimData) ValidationResult
        -DetermineValidationRoute(productCode, contractNumber) string
        -CallCNOUA(claimData) ValidationResult
        -CallSIPUA(claimData) ValidationResult
        -CallSIMDA(claimData) ValidationResult
    }

    class PhaseManagementService {
        <<Domain Service>>
        -IPhaseRepository phaseRepository
        -IPhaseEventRepository eventRepository
        +UpdatePhases(claimId, eventCode) void
        +OpenPhase(claimId, phaseCode) void
        +ClosePhase(claimId, phaseCode) void
        -GetPhaseConfiguration(eventCode) List~PhaseEventRelationship~
    }

    class SearchService {
        <<Domain Service>>
        -IClaimRepository claimRepository
        -IBranchRepository branchRepository
        -IPolicyRepository policyRepository
        +SearchByProtocol(source, protocol, dac) ClaimMaster
        +SearchByClaimNumber(origin, branch, number) ClaimMaster
        +SearchByLeaderCode(leaderCode, leaderClaim) ClaimMaster
        -LoadComplementaryData(claim) void
    }

    %% REPOSITORIES (INTERFACES)
    class IClaimRepository {
        <<Interface>>
        +FindByProtocol(source, protocol, dac) ClaimMaster
        +FindByClaimNumber(origin, branch, number) ClaimMaster
        +FindByLeaderCode(leaderCode, leaderClaim) ClaimMaster
        +Update(claim) void
        +BeginTransaction() ITransaction
        +CommitTransaction() void
        +RollbackTransaction() void
    }

    class IClaimHistoryRepository {
        <<Interface>>
        +Insert(history) void
        +GetHistoryByClaim(claimId) List~ClaimHistory~
        +GetTotalPaidByClaim(claimId) decimal
    }

    class ICurrencyUnitRepository {
        <<Interface>>
        +GetRateForDate(date) CurrencyUnit
        +IsRateAvailable(date) bool
    }

    class IPhaseRepository {
        <<Interface>>
        +GetOpenPhases(claimId) List~ClaimPhase~
        +OpenPhase(phase) void
        +ClosePhase(claimId, phaseCode, closingDate) void
    }

    %% VALIDATORS
    class PaymentAuthorizationValidator {
        <<Validator>>
        +ValidatePaymentType(type) ValidationError[]
        +ValidatePolicyType(type) ValidationError[]
        +ValidatePrincipalValue(value, pendingBalance) ValidationError[]
        +ValidateBeneficiary(beneficiary, insuranceType) ValidationError[]
        +Validate(authorization, claim) ValidationResult
    }

    class BusinessRuleValidator {
        <<Validator>>
        +ValidateAmountAgainstPending(amount, pending) bool
        +ValidateInsuranceTypeForBeneficiary(type, beneficiary) bool
        +ValidatePaymentTypeRange(type) bool
        +ValidatePolicyTypeRange(type) bool
    }

    %% RELACIONAMENTOS
    PaymentAuthorizationService --> IClaimRepository : uses
    PaymentAuthorizationService --> CurrencyConversionService : uses
    PaymentAuthorizationService --> ProductValidationService : uses
    PaymentAuthorizationService --> PhaseManagementService : uses
    PaymentAuthorizationService --> PaymentAuthorizationValidator : validates with

    CurrencyConversionService --> ICurrencyUnitRepository : uses
    ProductValidationService --> IConsortiumContractRepository : uses
    PhaseManagementService --> IPhaseRepository : uses
    SearchService --> IClaimRepository : uses

    PaymentAuthorizationValidator --> BusinessRuleValidator : uses
```

---

## Diagrama de Classes - Camada de API

```mermaid
classDiagram
    %% CONTROLLERS
    class ClaimSearchController {
        <<Controller>>
        -SearchService searchService
        -IMapper mapper
        +SearchByProtocol(request) IActionResult
        +SearchByClaimNumber(request) IActionResult
        +SearchByLeaderCode(request) IActionResult
    }

    class PaymentAuthorizationController {
        <<Controller>>
        -PaymentAuthorizationService authService
        -IMapper mapper
        +AuthorizePayment(request) IActionResult
        +GetAuthorizationHistory(claimId) IActionResult
    }

    class DashboardController {
        <<Controller>>
        -IDashboardService dashboardService
        +GetMigrationStatus() IActionResult
        +GetComponentStatus() IActionResult
        +GetPerformanceMetrics() IActionResult
    }

    %% DTOs - REQUEST
    class SearchByProtocolRequest {
        <<DTO>>
        +int Source
        +int ProtocolNumber
        +int DAC
        +void Validate()
    }

    class PaymentAuthorizationRequest {
        <<DTO>>
        +int ClaimId
        +int PaymentType
        +int PolicyType
        +decimal PrincipalValue
        +decimal CorrectionValue
        +string BeneficiaryName
        +void Validate()
    }

    %% DTOs - RESPONSE
    class ClaimDetailsResponse {
        <<DTO>>
        +string Protocol
        +string ClaimNumber
        +string BranchName
        +string InsuredName
        +decimal ExpectedReserve
        +decimal TotalPaid
        +decimal PendingValue
        +int ProductCode
        +int InsuranceType
    }

    class PaymentAuthorizationResponse {
        <<DTO>>
        +string TransactionId
        +bool Success
        +string Message
        +decimal AuthorizedAmount
        +decimal NewPendingBalance
        +DateTime AuthorizationDate
    }

    class ValidationErrorResponse {
        <<DTO>>
        +string ErrorCode
        +string Message
        +string Field
        +string Details
    }

    %% MIDDLEWARE
    class ExceptionHandlerMiddleware {
        <<Middleware>>
        +InvokeAsync(context, next) Task
        -HandleException(context, exception) Task
        -LogError(exception) void
    }

    class TransactionMiddleware {
        <<Middleware>>
        +InvokeAsync(context, next) Task
        -BeginTransaction() void
        -CommitOrRollback(success) void
    }

    %% RELACIONAMENTOS
    ClaimSearchController --> SearchService : uses
    ClaimSearchController --> SearchByProtocolRequest : receives
    ClaimSearchController --> ClaimDetailsResponse : returns

    PaymentAuthorizationController --> PaymentAuthorizationService : uses
    PaymentAuthorizationController --> PaymentAuthorizationRequest : receives
    PaymentAuthorizationController --> PaymentAuthorizationResponse : returns

    ExceptionHandlerMiddleware --> ValidationErrorResponse : creates
    TransactionMiddleware --> IClaimRepository : manages transaction
```

---

## Diagrama de Classes - Camada de Infraestrutura

```mermaid
classDiagram
    %% REPOSITORIES (IMPLEMENTATION)
    class ClaimRepository {
        <<Repository>>
        -SiweaDbContext context
        +FindByProtocol(source, protocol, dac) ClaimMaster
        +FindByClaimNumber(origin, branch, number) ClaimMaster
        +FindByLeaderCode(leaderCode, leaderClaim) ClaimMaster
        +Update(claim) void
        +BeginTransaction() ITransaction
        +CommitTransaction() void
        +RollbackTransaction() void
    }

    class ClaimHistoryRepository {
        <<Repository>>
        -SiweaDbContext context
        +Insert(history) void
        +GetHistoryByClaim(claimId) List~ClaimHistory~
        +GetTotalPaidByClaim(claimId) decimal
    }

    class CurrencyUnitRepository {
        <<Repository>>
        -SiweaDbContext context
        +GetRateForDate(date) CurrencyUnit
        +IsRateAvailable(date) bool
    }

    %% DB CONTEXT
    class SiweaDbContext {
        <<DbContext>>
        +DbSet~ClaimMaster~ ClaimMasters
        +DbSet~ClaimHistory~ ClaimHistories
        +DbSet~BranchMaster~ BranchMasters
        +DbSet~PolicyMaster~ PolicyMasters
        +DbSet~CurrencyUnit~ CurrencyUnits
        +DbSet~SystemControl~ SystemControls
        +DbSet~ClaimAccompaniment~ ClaimAccompaniments
        +DbSet~ClaimPhase~ ClaimPhases
        +DbSet~PhaseEventRelationship~ PhaseEventRelationships
        +DbSet~ConsortiumContract~ ConsortiumContracts
        #OnModelCreating(modelBuilder) void
    }

    %% ENTITY CONFIGURATIONS
    class ClaimMasterConfiguration {
        <<IEntityTypeConfiguration>>
        +Configure(builder) void
    }

    class ClaimHistoryConfiguration {
        <<IEntityTypeConfiguration>>
        +Configure(builder) void
    }

    %% EXTERNAL SERVICE CLIENTS
    class CNOUAClient {
        <<External Client>>
        -HttpClient httpClient
        -ICircuitBreakerPolicy circuitBreaker
        -IRetryPolicy retryPolicy
        +ValidateConsortiumProduct(request) ValidationResult
        -PrepareRequest(claimData) HttpRequestMessage
        -ParseResponse(response) ValidationResult
    }

    class SIPUAClient {
        <<External Client>>
        -ISoapClient soapClient
        -ICircuitBreakerPolicy circuitBreaker
        -IRetryPolicy retryPolicy
        +ValidateEFPContract(request) ValidationResult
        -BuildSoapEnvelope(claimData) string
        -ParseSoapResponse(xml) ValidationResult
    }

    class SIMDAClient {
        <<External Client>>
        -ISoapClient soapClient
        -ICircuitBreakerPolicy circuitBreaker
        -IRetryPolicy retryPolicy
        +ValidateHBContract(request) ValidationResult
        -BuildSoapEnvelope(claimData) string
        -ParseSoapResponse(xml) ValidationResult
    }

    %% RESILIENCE POLICIES
    class CircuitBreakerPolicy {
        <<Policy>>
        -int failureThreshold
        -TimeSpan openDuration
        -CircuitState state
        +ExecuteAsync(action) Task
        +OnFailure() void
        +OnSuccess() void
        +TryReset() bool
    }

    class RetryPolicy {
        <<Policy>>
        -int maxRetries
        -TimeSpan[] backoffIntervals
        +ExecuteAsync(action) Task
        -CalculateBackoff(attemptNumber) TimeSpan
    }

    %% RELACIONAMENTOS
    ClaimRepository --> SiweaDbContext : uses
    ClaimHistoryRepository --> SiweaDbContext : uses
    CurrencyUnitRepository --> SiweaDbContext : uses

    SiweaDbContext --> ClaimMasterConfiguration : uses
    SiweaDbContext --> ClaimHistoryConfiguration : uses

    ClaimRepository ..|> IClaimRepository : implements
    ClaimHistoryRepository ..|> IClaimHistoryRepository : implements
    CurrencyUnitRepository ..|> ICurrencyUnitRepository : implements

    CNOUAClient --> CircuitBreakerPolicy : uses
    CNOUAClient --> RetryPolicy : uses
    SIPUAClient --> CircuitBreakerPolicy : uses
    SIPUAClient --> RetryPolicy : uses
    SIMDAClient --> CircuitBreakerPolicy : uses
    SIMDAClient --> RetryPolicy : uses

    CNOUAClient ..|> ICNOUAClient : implements
    SIPUAClient ..|> ISIPUAClient : implements
    SIMDAClient ..|> ISIMDAClient : implements
```

---

## Diagrama de Classes - AutoMapper Profiles

```mermaid
classDiagram
    class ClaimMappingProfile {
        <<AutoMapper Profile>>
        +CreateMap~ClaimMaster,ClaimDetailsResponse~()
        +CreateMap~SearchByProtocolRequest,ProtocolSearchCriteria~()
        +CreateMap~PaymentAuthorizationRequest,PaymentAuthorization~()
    }

    class HistoryMappingProfile {
        <<AutoMapper Profile>>
        +CreateMap~ClaimHistory,HistoryItemResponse~()
        +CreateMap~PaymentAuthorization,ClaimHistory~()
    }

    ClaimMappingProfile --> ClaimMaster : maps from
    ClaimMappingProfile --> ClaimDetailsResponse : maps to
    HistoryMappingProfile --> ClaimHistory : maps from
```

---

## Diagrama de Classes - Validators (FluentValidation)

```mermaid
classDiagram
    class SearchByProtocolRequestValidator {
        <<AbstractValidator>>
        +SearchByProtocolRequestValidator()
        -ValidateSource() void
        -ValidateProtocolNumber() void
        -ValidateDAC() void
    }

    class PaymentAuthorizationRequestValidator {
        <<AbstractValidator>>
        +PaymentAuthorizationRequestValidator()
        -ValidatePaymentType() void
        -ValidatePolicyType() void
        -ValidatePrincipalValue() void
        -ValidateCorrectionValue() void
        -ValidateBeneficiary() void
    }

    SearchByProtocolRequestValidator --> SearchByProtocolRequest : validates
    PaymentAuthorizationRequestValidator --> PaymentAuthorizationRequest : validates
```

---

## Diagrama de Pacotes - Arquitetura Clean

```mermaid
classDiagram
    direction TB

    namespace CaixaSeguradora_Api {
        class Controllers {
            ClaimSearchController
            PaymentAuthorizationController
            DashboardController
        }
        class Middleware {
            ExceptionHandlerMiddleware
            TransactionMiddleware
        }
        class DTOs {
            SearchByProtocolRequest
            PaymentAuthorizationRequest
            ClaimDetailsResponse
        }
    }

    namespace CaixaSeguradora_Core {
        class Entities {
            ClaimMaster
            ClaimHistory
            BranchMaster
            PolicyMaster
        }
        class Services {
            PaymentAuthorizationService
            CurrencyConversionService
            ProductValidationService
        }
        class Interfaces {
            IClaimRepository
            ICurrencyUnitRepository
            IPhaseRepository
        }
        class Validators {
            PaymentAuthorizationValidator
            BusinessRuleValidator
        }
    }

    namespace CaixaSeguradora_Infrastructure {
        class Repositories {
            ClaimRepository
            ClaimHistoryRepository
            CurrencyUnitRepository
        }
        class DbContexts {
            SiweaDbContext
        }
        class ExternalClients {
            CNOUAClient
            SIPUAClient
            SIMDAClient
        }
        class Policies {
            CircuitBreakerPolicy
            RetryPolicy
        }
    }

    Controllers --> Services : uses
    Services --> Interfaces : depends on
    Repositories ..|> Interfaces : implements
    Repositories --> DbContexts : uses
    Services --> Validators : uses
    ExternalClients --> Policies : uses
```

**Dependências:**
- **Api → Core**: Controllers chamam Services
- **Core → Core**: Services usam Entities e Interfaces
- **Infrastructure → Core**: Repositories implementam Interfaces
- **Infrastructure → Infrastructure**: DbContext gerencia Entities

---

## Diagrama de Objetos - Exemplo de Autorização

```mermaid
classDiagram
    class claim_12345 {
        <<ClaimMaster instance>>
        InsuranceType = 1
        ClaimOrigin = 10
        ClaimBranch = 20
        ClaimNumber = 789012
        Source = 1
        ProtocolNumber = 123456
        DAC = 7
        ExpectedReserve = 50000.00
        TotalPaid = 10000.00
        OccurrenceNumber = 5
        ProductCode = 6814
    }

    class authorization_req {
        <<PaymentAuthorization instance>>
        PaymentType = 1
        PolicyType = 1
        PrincipalValue = 25000.00
        CorrectionValue = 500.00
        BeneficiaryName = "João da Silva"
    }

    class currency_rate {
        <<CurrencyUnit instance>>
        StartDate = 2024-10-01
        EndDate = 2024-10-31
        ConversionRate = 1.23456789
        CurrencyCode = "BTNF"
    }

    class converted_values {
        <<ConvertedValues instance>>
        PrincipalValueBTNF = 30864.20
        CorrectionValueBTNF = 617.28
        TotalValueBTNF = 31481.48
        ConversionRate = 1.23456789
    }

    class history_new {
        <<ClaimHistory instance>>
        InsuranceType = 1
        ClaimOrigin = 10
        ClaimBranch = 20
        ClaimNumber = 789012
        OccurrenceNumber = 6
        OperationCode = 1098
        PrincipalValue = 25000.00
        CorrectionValue = 500.00
        PrincipalValueBTNF = 30864.20
        CorrectionValueBTNF = 617.28
        TotalValueBTNF = 31481.48
        BeneficiaryName = "João da Silva"
        CorrectionType = "5"
        OperatorId = "OP12345"
    }

    claim_12345 --> authorization_req : receives
    authorization_req --> currency_rate : uses for conversion
    currency_rate --> converted_values : produces
    converted_values --> history_new : persisted as
    claim_12345 --> history_new : adds to history
```

**Fluxo:**
1. `claim_12345` (sinistro existente) recebe `authorization_req`
2. `currency_rate` (taxa do dia) converte valores
3. `converted_values` são calculados
4. `history_new` é criado e persistido
5. `claim_12345.TotalPaid` atualizado para 41481.48
6. `claim_12345.OccurrenceNumber` incrementado para 6

---

## Resumo de Classes por Camada

### API Layer (10 classes)
- **Controllers:** 3 (ClaimSearch, PaymentAuthorization, Dashboard)
- **DTOs Request:** 3 (SearchByProtocol, PaymentAuthorization, etc.)
- **DTOs Response:** 3 (ClaimDetails, PaymentAuthorization, ValidationError)
- **Middleware:** 2 (ExceptionHandler, Transaction)

### Core Layer (25+ classes)
- **Entities:** 13 (ClaimMaster, ClaimHistory, BranchMaster, etc.)
- **Value Objects:** 3 (PaymentAuthorization, ConvertedValues, ValidationResult)
- **Domain Services:** 5 (PaymentAuthorization, CurrencyConversion, ProductValidation, PhaseManagement, Search)
- **Repositories (Interfaces):** 6 (IClaim, IClaimHistory, ICurrencyUnit, IPhase, etc.)
- **Validators:** 2 (PaymentAuthorizationValidator, BusinessRuleValidator)

### Infrastructure Layer (15+ classes)
- **Repositories (Implementations):** 6 (ClaimRepository, ClaimHistoryRepository, etc.)
- **DbContext:** 1 (SiweaDbContext)
- **Entity Configurations:** 10+ (ClaimMasterConfiguration, etc.)
- **External Clients:** 3 (CNOUAClient, SIPUAClient, SIMDAClient)
- **Resilience Policies:** 2 (CircuitBreakerPolicy, RetryPolicy)

**Total:** ~50+ classes no sistema completo

---

## Padrões de Design Utilizados

| Padrão | Onde | Propósito |
|--------|------|-----------|
| **Repository** | Infrastructure | Abstração de acesso a dados |
| **Service Layer** | Core | Lógica de negócio centralizada |
| **DTO** | API | Transferência de dados entre camadas |
| **Dependency Injection** | Todas as camadas | Inversão de controle |
| **Unit of Work** | Infrastructure | Coordenar transações |
| **Circuit Breaker** | Infrastructure | Resiliência de integrações |
| **Retry** | Infrastructure | Tolerância a falhas temporárias |
| **Value Object** | Core | Objetos imutáveis sem identidade |
| **Entity** | Core | Objetos com identidade |
| **Factory** | Core | Criação de objetos complexos |
| **Validator** | Core/API | Validação de regras de negócio |
| **Mapper** | API | Conversão entre objetos |

---

**FIM DO DOCUMENTO - DIAGRAMAS DE CLASSES**
