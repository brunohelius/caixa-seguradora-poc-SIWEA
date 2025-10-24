# Research & Technical Decisions: Visual Age to .NET 9 Migration

**Branch**: `001-visualage-dotnet-migration` | **Date**: 2025-10-23 | **Phase**: 0 (Research)

## Executive Summary

This document captures all architectural decisions, technology selections, and implementation strategies for migrating the IBM VisualAge EZEE Claims Indemnity Payment Authorization System (SIWEA) to .NET 9 with React frontend. Each decision is grounded in industry best practices, official documentation, and the specific constraints of this legacy system migration project.

**Key Decisions Overview**:
1. **Database Access**: Entity Framework Core 9 with database-first approach
2. **SOAP Support**: SoapCore 1.x library with ASP.NET Core integration
3. **External Services**: HttpClient with Polly resilience policies
4. **Currency Conversion**: Domain service with decimal precision handling
5. **Dashboard Updates**: Polling-based approach with React Query
6. **Charting**: Recharts library for React-native visualizations
7. **Testing**: Multi-layered strategy with snapshot testing for parity
8. **Authentication**: JWT-based authentication with Active Directory integration

---

## 1. Database Access Strategy

### Decision

**Entity Framework Core 9.0** with **database-first approach** using reverse engineering from existing DB2/SQL Server schema. Connection managed via **IBM.Data.DB2.Core** (if DB2) or **Microsoft.Data.SqlClient** (if SQL Server) providers.

### Rationale

Entity Framework Core is the standard, mature ORM for .NET applications and provides:

- **Zero Schema Changes**: Database-first approach allows mapping to existing legacy tables without migrations
- **Fluent API Configuration**: Fine-grained control over table/column mappings for legacy naming conventions
- **Transaction Support**: Built-in transaction management for multi-table updates (claim master + history + phase)
- **Performance**: Mature query optimization, compiled queries, and connection pooling
- **Type Safety**: Strongly-typed LINQ queries eliminate SQL injection risks
- **Maintainability**: Clear separation between data access and business logic via Repository pattern

The database-first workflow preserves the existing schema while providing modern ORM capabilities:

```bash
# Scaffold existing database to EF Core entities
dotnet ef dbcontext scaffold "Server=db2server;Database=CLAIMS;..." \
  IBM.Data.DB2.Core \
  --output-dir Entities \
  --context ClaimsDbContext \
  --data-annotations \
  --force
```

### Alternatives Considered

#### Alternative A: Dapper (Micro-ORM)
**Pros**:
- Lightweight and extremely fast (10-15% faster than EF Core for simple queries)
- Direct SQL control for complex queries
- Minimal abstraction overhead

**Cons**:
- Manual mapping required for all entities (13 entities × ~10-20 properties each)
- No change tracking or transaction management
- Requires hand-written SQL for CRUD operations
- No built-in support for complex relationships
- Higher maintenance burden for schema changes

**Why Not Chosen**: The project has 13 complex entities with relationships. Manual mapping and SQL would introduce significant technical debt. Performance difference is negligible for this workload (1000 concurrent users, not millions).

#### Alternative B: ADO.NET (Raw SQL)
**Pros**:
- Maximum performance control
- No ORM abstraction
- Works with any database

**Cons**:
- Verbose boilerplate code for every query
- High risk of SQL injection without parameterization discipline
- No compile-time safety for queries
- Manual transaction management
- Difficult to test (tight coupling to database)

**Why Not Chosen**: Modern best practices favor ORMs for maintainability and security. The performance benefits are minimal compared to development cost.

#### Alternative C: NHibernate
**Pros**:
- Mature ORM with extensive features
- XML-based or attribute-based mappings
- Strong community support

**Cons**:
- Not officially maintained by Microsoft (.NET ecosystem fragmentation)
- More complex configuration than EF Core
- Smaller community and fewer resources compared to EF Core
- Steeper learning curve for .NET developers

**Why Not Chosen**: EF Core is the de facto standard in .NET ecosystem with better Microsoft support and documentation.

### Implementation Notes

#### 1. Provider Selection

For **DB2**:
```xml
<PackageReference Include="IBM.Data.DB2.Core" Version="3.1.0.500" />
```

For **SQL Server**:
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
```

#### 2. DbContext Configuration

```csharp
public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options) { }

    public DbSet<ClaimMaster> ClaimMasters { get; set; }
    public DbSet<ClaimHistory> ClaimHistories { get; set; }
    // ... other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Legacy table mapping with Fluent API
        modelBuilder.Entity<ClaimMaster>(entity =>
        {
            entity.ToTable("TMESTSIN"); // Legacy table name
            entity.HasKey(e => e.NumSinistro);

            entity.Property(e => e.NumSinistro)
                .HasColumnName("NUMSINISTRO") // Legacy column name
                .IsRequired();

            entity.Property(e => e.ValorPrincipal)
                .HasColumnName("VALPRI")
                .HasColumnType("decimal(15,2)"); // Explicit precision

            // Relationships
            entity.HasMany(e => e.Histories)
                .WithOne(h => h.ClaimMaster)
                .HasForeignKey(h => h.NumSinistro)
                .OnDelete(DeleteBehavior.Restrict); // Preserve referential integrity
        });

        // Apply all configurations from separate files
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }
}
```

#### 3. Connection String Management

Store connection strings in **Azure Key Vault** for production, appsettings for development:

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "ClaimsDatabase": "Server=localhost;Database=CLAIMS;User Id=dev_user;Password=dev_pass;TrustServerCertificate=true;"
  }
}
```

```csharp
// Program.cs - Production configuration
var connectionString = builder.Configuration["ConnectionStrings:ClaimsDatabase"];

if (builder.Environment.IsProduction())
{
    // Retrieve from Azure Key Vault
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
    connectionString = builder.Configuration["ClaimsDatabase-ConnectionString"];
}

builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseSqlServer(connectionString) // or UseDB2() for DB2
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .EnableDetailedErrors(builder.Environment.IsDevelopment())
);
```

#### 4. Transaction Handling

Multi-table updates require explicit transactions:

```csharp
public async Task<PaymentAuthorization> CreatePaymentAuthorizationAsync(
    PaymentAuthorizationRequest request,
    string userId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // 1. Update claim master
        var claim = await _context.ClaimMasters.FindAsync(request.ClaimNumber);
        claim.Status = "AUTHORIZED";
        claim.LastModifiedBy = userId;
        claim.LastModifiedDate = DateTime.Now;

        // 2. Insert history record
        var history = new ClaimHistory
        {
            NumSinistro = claim.NumSinistro,
            DateMovement = DateTime.Now,
            UserID = userId,
            Action = "PAYMENT_AUTH",
            // ... other fields
        };
        _context.ClaimHistories.Add(history);

        // 3. Update phase
        var phase = await _context.ClaimPhases
            .FirstOrDefaultAsync(p => p.ClaimNumber == claim.NumSinistro);
        phase.CurrentPhase = "PAGAMENTO_AUTORIZADO";

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToPaymentAuthorization(claim, history);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Failed to create payment authorization for claim {ClaimNumber}",
            request.ClaimNumber);
        throw;
    }
}
```

#### 5. Repository Pattern Implementation

```csharp
public interface IClaimRepository
{
    Task<ClaimMaster?> GetByClaimNumberAsync(string claimNumber);
    Task<IEnumerable<ClaimMaster>> SearchByProtocolAsync(string protocol);
    Task<IEnumerable<ClaimHistory>> GetClaimHistoryAsync(string claimNumber);
    Task<int> SaveChangesAsync();
}

public class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimMaster?> GetByClaimNumberAsync(string claimNumber)
    {
        return await _context.ClaimMasters
            .Include(c => c.BranchMaster)
            .Include(c => c.PolicyMaster)
            .Include(c => c.Accompaniment)
            .AsNoTracking() // Read-only query optimization
            .FirstOrDefaultAsync(c => c.NumSinistro == claimNumber);
    }

    // ... other methods
}
```

#### 6. Performance Optimization

```csharp
// Compiled queries for frequently used searches
private static readonly Func<ClaimsDbContext, string, Task<ClaimMaster?>>
    CompiledGetByClaimNumber = EF.CompileAsyncQuery(
        (ClaimsDbContext context, string claimNumber) =>
            context.ClaimMasters
                .Include(c => c.BranchMaster)
                .FirstOrDefault(c => c.NumSinistro == claimNumber)
    );

// Indexing for search fields (if allowed to add indexes)
modelBuilder.Entity<ClaimMaster>()
    .HasIndex(e => e.ProtocolNumber)
    .HasDatabaseName("IX_ClaimMaster_Protocol");

// Pagination for large result sets
public async Task<PagedResult<ClaimMaster>> SearchClaimsAsync(
    ClaimSearchCriteria criteria,
    int page = 1,
    int pageSize = 50)
{
    var query = _context.ClaimMasters.AsQueryable();

    if (!string.IsNullOrEmpty(criteria.Protocol))
        query = query.Where(c => c.ProtocolNumber == criteria.Protocol);

    var total = await query.CountAsync();
    var items = await query
        .OrderByDescending(c => c.CreatedDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();

    return new PagedResult<ClaimMaster>(items, total, page, pageSize);
}
```

### Risks/Tradeoffs

1. **Performance with Large Datasets**
   - **Risk**: EF Core may generate inefficient SQL for complex queries
   - **Mitigation**: Use `.AsNoTracking()` for read-only queries, compiled queries, and profiling with SQL Server Profiler or DB2 traces

2. **Legacy Column Naming Conventions**
   - **Risk**: All-caps legacy column names require verbose Fluent API configurations
   - **Mitigation**: Use separate configuration classes (IEntityTypeConfiguration<T>) to keep DbContext clean

3. **Concurrent Updates**
   - **Risk**: Optimistic concurrency violations when multiple operators edit the same claim
   - **Mitigation**: Implement concurrency tokens (RowVersion/Timestamp) and retry logic with user notification

4. **Database Provider Differences**
   - **Risk**: DB2 provider may have limited LINQ support compared to SQL Server
   - **Mitigation**: Write provider-agnostic queries, test against both databases, fallback to raw SQL for DB2-specific features

**Official References**:
- [EF Core 9 Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Database-First Approach](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding)
- [IBM.Data.DB2.Core NuGet](https://www.nuget.org/packages/IBM.Data.DB2.Core/)
- [EF Core Performance Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)

---

## 2. SOAP Endpoint Implementation

### Decision

**SoapCore 1.x library** integrated with ASP.NET Core Web API to expose SOAP endpoints alongside REST APIs. Uses WCF-like service contracts with shared DTOs between REST and SOAP.

### Rationale

SoapCore is the de facto standard for SOAP in .NET Core/.NET 5+ applications because:

- **Seamless Integration**: Works within ASP.NET Core middleware pipeline (same authentication, logging, DI container)
- **No Manual WSDL**: Automatically generates WSDL from service contracts at runtime
- **Shared Infrastructure**: SOAP and REST endpoints share the same controllers/services (no code duplication)
- **WCF Compatibility**: Uses familiar `[ServiceContract]` and `[OperationContract]` attributes
- **HTTPS/TLS Support**: Native ASP.NET Core TLS handling
- **Active Maintenance**: Regularly updated for new .NET versions

This approach allows legacy SOAP clients (CICS systems, Visual Age integration points) to consume the new .NET API without modifications while modern clients can use REST.

### Alternatives Considered

#### Alternative A: CoreWCF (Official WCF Port)
**Pros**:
- Official Microsoft-supported WCF port to .NET Core
- Full WCF feature parity (transactions, security, duplex channels)
- Better compatibility with complex WCF scenarios

**Cons**:
- Heavier dependency (full WCF stack vs lightweight SoapCore)
- More complex configuration than needed for basic SOAP
- Overkill for simple request/response SOAP services
- Requires separate hosting from REST API (different endpoints)

**Why Not Chosen**: This migration only needs basic SOAP request/response patterns with authentication. CoreWCF's complexity is unnecessary overhead.

#### Alternative B: Manual WSDL Generation with ASP.NET Core
**Pros**:
- Full control over WSDL structure
- No third-party dependencies
- Custom SOAP envelope handling

**Cons**:
- Requires hand-crafting WSDL files (error-prone)
- Manual XML serialization/deserialization logic
- No automatic binding generation
- High maintenance burden for schema changes
- Must manually implement SOAP fault handling

**Why Not Chosen**: Manual WSDL generation is time-consuming and error-prone. SoapCore automates this entirely.

#### Alternative C: Azure API Management SOAP Passthrough
**Pros**:
- No code changes needed
- API gateway handles SOAP-to-REST translation
- Centralized policy management

**Cons**:
- Vendor lock-in to Azure
- Additional infrastructure cost
- Latency overhead (extra network hop)
- Requires Visual Age SOAP interface to remain operational

**Why Not Chosen**: The goal is to migrate away from Visual Age entirely. API Management adds complexity and doesn't allow decommissioning the legacy system.

### Implementation Notes

#### 1. NuGet Package Installation

```xml
<PackageReference Include="SoapCore" Version="1.1.0" />
```

#### 2. Service Contract Definition

```csharp
// Define SOAP service contract
[ServiceContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1")]
public interface IAutenticacaoService
{
    [OperationContract]
    Task<AutenticacaoResponse> AutenticarAsync(AutenticacaoRequest request);
}

[ServiceContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")]
public interface ISolicitacaoService
{
    [OperationContract]
    Task<SolicitacaoResponse> CriarSolicitacaoAsync(SolicitacaoRequest request);

    [OperationContract]
    Task<ConsultaSolicitacaoResponse> ConsultarSolicitacaoAsync(ConsultaSolicitacaoRequest request);
}

[ServiceContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0003_Assunto/v1")]
public interface IAssuntoService
{
    [OperationContract]
    Task<AssuntoResponse> ListarAssuntosAsync(AssuntoRequest request);
}
```

#### 3. DTO Definitions (Shared between REST and SOAP)

```csharp
[DataContract]
public class AutenticacaoRequest
{
    [DataMember(Order = 1)]
    public string CodUsuario { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string DesSenha { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string CodSistema { get; set; } = string.Empty;
}

[DataContract]
public class AutenticacaoResponse
{
    [DataMember(Order = 1)]
    public string SessionId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public bool Sucesso { get; set; }

    [DataMember(Order = 3)]
    public string MensagemErro { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public DateTime DataExpiracao { get; set; }
}
```

#### 4. Service Implementation

```csharp
public class AutenticacaoService : IAutenticacaoService
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AutenticacaoService> _logger;

    public AutenticacaoService(
        IAuthenticationService authService,
        ILogger<AutenticacaoService> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<AutenticacaoResponse> AutenticarAsync(AutenticacaoRequest request)
    {
        try
        {
            _logger.LogInformation(
                "SOAP Authentication request for user {User} in system {System}",
                request.CodUsuario,
                request.CodSistema);

            var result = await _authService.AuthenticateAsync(
                request.CodUsuario,
                request.DesSenha,
                request.CodSistema);

            return new AutenticacaoResponse
            {
                SessionId = result.SessionId,
                Sucesso = result.Success,
                MensagemErro = result.ErrorMessage ?? string.Empty,
                DataExpiracao = result.ExpirationDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SOAP authentication failed for user {User}",
                request.CodUsuario);

            return new AutenticacaoResponse
            {
                Sucesso = false,
                MensagemErro = "Erro interno no serviço de autenticação"
            };
        }
    }
}
```

#### 5. Program.cs Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register SOAP services
builder.Services.AddSingleton<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddSingleton<ISolicitacaoService, SolicitacaoService>();
builder.Services.AddSingleton<IAssuntoService, AssuntoService>();

// Add SoapCore
builder.Services.AddSoapCore();

var app = builder.Build();

// Configure SOAP endpoints
app.UseSoapEndpoint<IAutenticacaoService>(
    "/soap/autenticacao",
    new SoapEncoderOptions
    {
        MessageVersion = MessageVersion.Soap11, // or Soap12
        WriteEncoding = Encoding.UTF8
    },
    SoapSerializer.DataContractSerializer);

app.UseSoapEndpoint<ISolicitacaoService>(
    "/soap/solicitacao",
    new SoapEncoderOptions
    {
        MessageVersion = MessageVersion.Soap11,
        WriteEncoding = Encoding.UTF8
    },
    SoapSerializer.DataContractSerializer);

app.UseSoapEndpoint<IAssuntoService>(
    "/soap/assunto",
    new SoapEncoderOptions
    {
        MessageVersion = MessageVersion.Soap11,
        WriteEncoding = Encoding.UTF8
    },
    SoapSerializer.DataContractSerializer);

app.Run();
```

#### 6. WSDL Access

WSDL is automatically generated at:
- `https://localhost:5001/soap/autenticacao?wsdl`
- `https://localhost:5001/soap/solicitacao?wsdl`
- `https://localhost:5001/soap/assunto?wsdl`

#### 7. SOAP Fault Handling

```csharp
public class SoapExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SoapExceptionMiddleware> _logger;

    public SoapExceptionMiddleware(
        RequestDelegate next,
        ILogger<SoapExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Only handle SOAP endpoints
            if (context.Request.Path.StartsWithSegments("/soap"))
            {
                _logger.LogError(ex, "SOAP endpoint error: {Path}", context.Request.Path);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/xml; charset=utf-8";

                var soapFault = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <soap:Fault>
            <faultcode>soap:Server</faultcode>
            <faultstring>{System.Security.SecurityElement.Escape(ex.Message)}</faultstring>
            <detail>
                <ErrorCode>INTERNAL_ERROR</ErrorCode>
            </detail>
        </soap:Fault>
    </soap:Body>
</soap:Envelope>";

                await context.Response.WriteAsync(soapFault);
            }
            else
            {
                throw; // Re-throw for non-SOAP endpoints
            }
        }
    }
}

// Register in Program.cs
app.UseMiddleware<SoapExceptionMiddleware>();
```

#### 8. Authentication Integration

SOAP endpoints can share the same JWT authentication as REST:

```csharp
// Option 1: Session-based (legacy compatibility)
app.UseSoapEndpoint<IAutenticacaoService>(
    "/soap/autenticacao",
    new SoapEncoderOptions(),
    SoapSerializer.DataContractSerializer,
    omitXmlDeclaration: false,
    indentXml: false,
    caseInsensitivePath: true,
    // Custom authentication handler
    authenticator: (httpContext) =>
    {
        var sessionId = httpContext.Request.Headers["X-Session-Id"].ToString();
        return _sessionService.ValidateSession(sessionId);
    });

// Option 2: HTTP Basic Auth (simpler for legacy clients)
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        "BasicAuthentication", null);
```

### Risks/Tradeoffs

1. **Limited SOAP Features**
   - **Risk**: SoapCore doesn't support advanced WCF features (WS-*, duplex, transactions)
   - **Mitigation**: This project only needs basic request/response. If advanced features needed later, migrate to CoreWCF

2. **WSDL Compatibility with Legacy Clients**
   - **Risk**: Auto-generated WSDL may differ from Visual Age WSDL structure
   - **Mitigation**: Test with actual CICS clients, compare WSDLs side-by-side, customize with SoapCore options if needed

3. **Namespace Mapping**
   - **Risk**: Legacy clients may expect exact namespace URIs
   - **Mitigation**: Use `[ServiceContract(Namespace = "...")]` to match Visual Age namespaces exactly

4. **Performance Overhead**
   - **Risk**: XML serialization/deserialization is slower than JSON (REST)
   - **Mitigation**: Acceptable tradeoff for legacy compatibility. Encourage new integrations to use REST

**Official References**:
- [SoapCore GitHub Repository](https://github.com/DigDes/SoapCore)
- [CoreWCF Documentation](https://github.com/CoreWCF/CoreWCF)
- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)

---

## 3. External Service Integration

### Decision

**HttpClient** with **Polly v8** resilience policies (retry, circuit breaker, timeout) for integrating with CNOUA, SIPUA, and SIMDA external validation services. Typed clients registered via dependency injection with configuration-based endpoints.

### Rationale

HttpClient + Polly is the recommended pattern for .NET HTTP communication because:

- **Built-in Connection Pooling**: HttpClientFactory manages socket lifecycle to prevent port exhaustion
- **Resilience Patterns**: Polly provides battle-tested retry, circuit breaker, timeout, and fallback policies
- **Typed Clients**: Strongly-typed interfaces for each external service (ICnouaClient, ISipuaClient, ISimdaClient)
- **Configuration-Based**: Service URLs and timeouts managed in appsettings.json (environment-specific)
- **Testability**: Easy to mock for unit tests
- **Telemetry**: Integrates with Application Insights, Serilog for distributed tracing

The circuit breaker pattern is critical for external dependencies - if CNOUA is down, we fail fast rather than overwhelming it with retries.

### Alternatives Considered

#### Alternative A: RestSharp Library
**Pros**:
- Higher-level API than HttpClient (easier serialization)
- Built-in support for XML, JSON, authentication
- Fluent API for request building

**Cons**:
- Additional dependency (HttpClient is in .NET runtime)
- Less control over connection pooling
- No native resilience policies (would still need Polly)
- Community-maintained (not Microsoft)

**Why Not Chosen**: HttpClient with Polly is the .NET standard. RestSharp adds no significant value for simple HTTP calls.

#### Alternative B: Refit (Type-Safe REST Client)
**Pros**:
- Auto-generates HTTP clients from interfaces
- Very clean API (no boilerplate)
- Works well with Polly

**Cons**:
- Requires external services to follow REST conventions (may not be the case)
- Less flexible for custom SOAP/XML handling
- Learning curve for team unfamiliar with Refit

**Why Not Chosen**: Unknown if CNOUA/SIPUA/SIMDA are REST-based. HttpClient is more flexible for any protocol (SOAP, REST, custom XML).

#### Alternative C: gRPC
**Pros**:
- High performance (binary protocol)
- Strong typing with Protobuf
- Bi-directional streaming

**Cons**:
- Requires external services to support gRPC (highly unlikely for legacy systems)
- Not suitable for HTTP/SOAP services
- Overkill for simple request/response

**Why Not Chosen**: Legacy enterprise systems use HTTP/SOAP, not gRPC.

### Implementation Notes

#### 1. NuGet Packages

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="9.0.0" />
<PackageReference Include="Polly" Version="8.4.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
```

#### 2. Service Client Interface

```csharp
public interface ICnouaClient
{
    Task<CnouaValidationResponse> ValidateClaimAsync(
        string claimNumber,
        CancellationToken cancellationToken = default);
}

public interface ISipuaClient
{
    Task<SipuaValidationResponse> ValidatePolicyAsync(
        string policyNumber,
        CancellationToken cancellationToken = default);
}

public interface ISimdaClient
{
    Task<SimdaValidationResponse> ValidateConsortiumAsync(
        string contractNumber,
        CancellationToken cancellationToken = default);
}
```

#### 3. Client Implementation

```csharp
public class CnouaClient : ICnouaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CnouaClient> _logger;

    public CnouaClient(HttpClient httpClient, ILogger<CnouaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CnouaValidationResponse> ValidateClaimAsync(
        string claimNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating claim {ClaimNumber} with CNOUA", claimNumber);

        var request = new CnouaValidationRequest
        {
            ClaimNumber = claimNumber,
            RequestTimestamp = DateTime.UtcNow
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/validate-claim",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CnouaValidationResponse>(
            cancellationToken: cancellationToken);

        if (result == null)
            throw new InvalidOperationException("CNOUA returned null response");

        // Check for error code (EZERT8 != '00000000')
        if (result.ErrorCode != "00000000")
        {
            _logger.LogWarning(
                "CNOUA validation failed for claim {ClaimNumber}: {ErrorCode} - {ErrorMessage}",
                claimNumber,
                result.ErrorCode,
                result.ErrorMessage);

            throw new ExternalServiceValidationException(
                $"CNOUA validation error: {result.ErrorMessage}",
                result.ErrorCode);
        }

        return result;
    }
}
```

#### 4. Polly Resilience Policies

```csharp
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

// Program.cs - Register typed clients with Polly policies
builder.Services.AddHttpClient<ICnouaClient, CnouaClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:CNOUA:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "CaixaSeguradora-API/1.0");
})
.AddStandardResilienceHandler(options =>
{
    // Retry policy: 3 attempts with exponential backoff
    options.Retry = new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = args =>
        {
            var logger = args.Context.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(
                "CNOUA retry attempt {Attempt} after {Delay}ms due to: {Exception}",
                args.AttemptNumber,
                args.RetryDelay.TotalMilliseconds,
                args.Outcome.Exception?.Message);
            return ValueTask.CompletedTask;
        }
    };

    // Circuit breaker: Open after 5 consecutive failures, half-open after 30s
    options.CircuitBreaker = new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5, // 50% failure rate
        MinimumThroughput = 10, // At least 10 requests before evaluating
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(30),
        OnOpened = args =>
        {
            var logger = args.Context.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError("CNOUA circuit breaker OPENED due to failures");
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            var logger = args.Context.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("CNOUA circuit breaker CLOSED - service recovered");
            return ValueTask.CompletedTask;
        }
    };

    // Timeout: 10 seconds per attempt
    options.AttemptTimeout = new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    // Total timeout: 45 seconds for all retries
    options.TotalRequestTimeout = new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(45)
    };
});

// Similar configuration for SIPUA and SIMDA
builder.Services.AddHttpClient<ISipuaClient, SipuaClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:SIPUA:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler(); // Uses default resilience settings

builder.Services.AddHttpClient<ISimdaClient, SimdaClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:SIMDA:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();
```

#### 5. Configuration (appsettings.json)

```json
{
  "ExternalServices": {
    "CNOUA": {
      "BaseUrl": "https://cnoua-api.caixaseguradora.com.br",
      "Timeout": 30,
      "RetryCount": 3,
      "CircuitBreakerThreshold": 5
    },
    "SIPUA": {
      "BaseUrl": "https://sipua-api.caixaseguradora.com.br",
      "Timeout": 30,
      "RetryCount": 3,
      "CircuitBreakerThreshold": 5
    },
    "SIMDA": {
      "BaseUrl": "https://simda-api.caixaseguradora.com.br",
      "Timeout": 30,
      "RetryCount": 3,
      "CircuitBreakerThreshold": 5
    }
  }
}
```

#### 6. Error Code Mapping

```csharp
public class ExternalServiceValidationException : Exception
{
    public string ErrorCode { get; }

    public ExternalServiceValidationException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

// Map external error codes to user-friendly Portuguese messages
public static class ErrorCodeMapper
{
    private static readonly Dictionary<string, string> ErrorMessages = new()
    {
        { "00000000", "Validação bem-sucedida" },
        { "00000001", "Sinistro não encontrado no sistema externo" },
        { "00000002", "Apólice inválida ou cancelada" },
        { "00000003", "Contrato de consórcio não localizado" },
        { "00000004", "Prazo de validação expirado" },
        { "99999999", "Erro interno no serviço de validação" }
    };

    public static string GetMessage(string errorCode)
    {
        return ErrorMessages.TryGetValue(errorCode, out var message)
            ? message
            : $"Erro desconhecido: {errorCode}";
    }
}
```

#### 7. SOAP Service Integration (if needed)

If external services use SOAP instead of REST:

```csharp
public class CnouaSoapClient : ICnouaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CnouaSoapClient> _logger;

    public async Task<CnouaValidationResponse> ValidateClaimAsync(
        string claimNumber,
        CancellationToken cancellationToken = default)
    {
        var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ValidateClaim xmlns=""http://cnoua.caixaseguradora.com.br/"">
            <claimNumber>{claimNumber}</claimNumber>
        </ValidateClaim>
    </soap:Body>
</soap:Envelope>";

        var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "http://cnoua.caixaseguradora.com.br/ValidateClaim");

        var response = await _httpClient.PostAsync("/services/validation", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var xmlResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        // Parse SOAP response (use XDocument or XmlSerializer)
        return ParseSoapResponse(xmlResponse);
    }
}
```

#### 8. Testing with Mock Clients

```csharp
public class MockCnouaClient : ICnouaClient
{
    public Task<CnouaValidationResponse> ValidateClaimAsync(
        string claimNumber,
        CancellationToken cancellationToken = default)
    {
        // Return successful validation for testing
        return Task.FromResult(new CnouaValidationResponse
        {
            ErrorCode = "00000000",
            ErrorMessage = "Validação bem-sucedida",
            IsValid = true,
            ValidationTimestamp = DateTime.UtcNow
        });
    }
}

// Register in Program.cs for Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<ICnouaClient, MockCnouaClient>();
    builder.Services.AddSingleton<ISipuaClient, MockSipuaClient>();
    builder.Services.AddSingleton<ISimdaClient, MockSimdaClient>();
}
```

### Risks/Tradeoffs

1. **Unknown Service Protocols**
   - **Risk**: Specification doesn't define if CNOUA/SIPUA/SIMDA are REST, SOAP, or proprietary
   - **Mitigation**: Request API documentation from service owners, prepare adapters for different protocols

2. **Network Failures Impact Business Logic**
   - **Risk**: External service downtime blocks payment authorizations (FR-024, FR-030)
   - **Mitigation**: Circuit breaker fails fast, consider fallback workflows (e.g., manual approval queue)

3. **Timeout Tuning**
   - **Risk**: 30-second timeout may be too short/long depending on service SLAs
   - **Mitigation**: Monitor P95/P99 latencies in production, adjust timeouts based on actual performance

4. **Error Code Standardization**
   - **Risk**: Each service may use different error code formats
   - **Mitigation**: Create adapter layer to normalize error codes to common format

**Official References**:
- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Polly Documentation](https://www.pollydocs.org/)
- [Resilience Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)

---

## 4. Currency Conversion Logic

### Decision

**Domain Service** (`CurrencyConversionService`) in the Core layer with **decimal data type** for all monetary calculations. Uses `TGEUNIMO` table for BTNF rate lookup with date range validation.

### Rationale

Currency conversion is core business logic that must be:

- **Accurate**: C# `decimal` type provides 28-29 significant digits (sufficient for 2 decimal place requirement)
- **Testable**: Pure function logic isolated from database and UI
- **Auditable**: Clear calculation formula matching specification: `VALPRIBT = VALPRI * VLCRUZAD`
- **Domain-Driven**: Encapsulates business rules in domain service, not in repository or controller

The `decimal` type is mandatory for financial calculations in .NET to avoid floating-point precision errors (e.g., `0.1 + 0.2 != 0.3` with `double`).

### Alternatives Considered

#### Alternative A: Third-Party Library (NodaMoney, Money.NET)
**Pros**:
- Pre-built currency types with rounding rules
- Multi-currency support
- Arithmetic operator overloads

**Cons**:
- Overkill for single conversion (BRL to BTNF)
- Additional dependency
- Learning curve for team
- May not match legacy rounding behavior exactly

**Why Not Chosen**: Simple formula doesn't justify external library. `decimal` is sufficient.

#### Alternative B: Stored Procedure/Database Function
**Pros**:
- Centralized logic (both .NET and legacy systems could call it)
- Database-level precision control
- Potentially faster (no network round-trip)

**Cons**:
- Violates Clean Architecture (business logic in infrastructure)
- Harder to unit test
- Database-specific (not portable)
- Specification requires zero schema changes

**Why Not Chosen**: Specification prohibits schema changes. Business logic belongs in application layer.

#### Alternative C: JavaScript/Frontend Calculation
**Pros**:
- Immediate user feedback without API call
- Reduces server load

**Cons**:
- JavaScript `Number` has precision issues (floating-point)
- Business logic duplication (frontend + backend)
- Security risk (client can manipulate calculations)
- Audit trail on server-side only

**Why Not Chosen**: Financial calculations must be server-authoritative. Frontend can display preview, but server validates.

### Implementation Notes

#### 1. Domain Entity for Currency Unit

```csharp
public class CurrencyUnit
{
    public int Id { get; set; }

    [Required]
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = string.Empty; // "BRL", "BTNF"

    public decimal ExchangeRate { get; set; } // VLCRUZAD column

    public DateTime EffectiveStartDate { get; set; } // DTINIVIG

    public DateTime EffectiveEndDate { get; set; } // DTTERVIG

    public bool IsActive { get; set; }

    // Navigation properties
    public int BranchCode { get; set; }
    public BranchMaster BranchMaster { get; set; } = null!;
}
```

#### 2. Currency Conversion Service Interface

```csharp
public interface ICurrencyConversionService
{
    Task<decimal> ConvertToBtnfAsync(
        decimal amount,
        DateTime transactionDate,
        int branchCode);

    Task<CurrencyConversionResult> ConvertWithDetailsAsync(
        decimal amount,
        DateTime transactionDate,
        int branchCode);
}

public class CurrencyConversionResult
{
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime RateEffectiveDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

#### 3. Service Implementation

```csharp
public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<CurrencyConversionService> _logger;

    public CurrencyConversionService(
        ICurrencyRepository currencyRepository,
        ILogger<CurrencyConversionService> logger)
    {
        _currencyRepository = currencyRepository;
        _logger = logger;
    }

    public async Task<decimal> ConvertToBtnfAsync(
        decimal amount,
        DateTime transactionDate,
        int branchCode)
    {
        var result = await ConvertWithDetailsAsync(amount, transactionDate, branchCode);
        return result.ConvertedAmount;
    }

    public async Task<CurrencyConversionResult> ConvertWithDetailsAsync(
        decimal amount,
        DateTime transactionDate,
        int branchCode)
    {
        _logger.LogInformation(
            "Converting {Amount} to BTNF for transaction date {Date} in branch {Branch}",
            amount,
            transactionDate,
            branchCode);

        // Lookup exchange rate from TGEUNIMO table
        // WHERE DTINIVIG <= transactionDate <= DTTERVIG AND BranchCode = branchCode
        var currencyUnit = await _currencyRepository.GetExchangeRateAsync(
            "BTNF",
            transactionDate,
            branchCode);

        if (currencyUnit == null)
        {
            var message = $"Taxa de conversão BTNF não encontrada para a data {transactionDate:dd/MM/yyyy}";
            _logger.LogWarning(message);

            throw new CurrencyConversionException(message);
        }

        // Apply formula: VALPRIBT = VALPRI * VLCRUZAD
        var convertedAmount = amount * currencyUnit.ExchangeRate;

        // Round to 2 decimal places (SC-008 requirement)
        convertedAmount = Math.Round(convertedAmount, 2, MidpointRounding.AwayFromZero);

        _logger.LogInformation(
            "Conversion successful: {Original} * {Rate} = {Converted}",
            amount,
            currencyUnit.ExchangeRate,
            convertedAmount);

        return new CurrencyConversionResult
        {
            OriginalAmount = amount,
            ConvertedAmount = convertedAmount,
            ExchangeRate = currencyUnit.ExchangeRate,
            TransactionDate = transactionDate,
            RateEffectiveDate = currencyUnit.EffectiveStartDate,
            Message = "Conversão realizada com sucesso"
        };
    }
}

public class CurrencyConversionException : Exception
{
    public CurrencyConversionException(string message) : base(message) { }
}
```

#### 4. Repository Implementation

```csharp
public interface ICurrencyRepository
{
    Task<CurrencyUnit?> GetExchangeRateAsync(
        string currencyCode,
        DateTime effectiveDate,
        int branchCode);
}

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ClaimsDbContext _context;

    public CurrencyRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyUnit?> GetExchangeRateAsync(
        string currencyCode,
        DateTime effectiveDate,
        int branchCode)
    {
        return await _context.CurrencyUnits
            .Where(cu =>
                cu.CurrencyCode == currencyCode &&
                cu.BranchCode == branchCode &&
                cu.EffectiveStartDate <= effectiveDate &&
                cu.EffectiveEndDate >= effectiveDate &&
                cu.IsActive)
            .OrderByDescending(cu => cu.EffectiveStartDate) // Get most recent rate if multiple
            .FirstOrDefaultAsync();
    }
}
```

#### 5. Controller Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;
    private readonly ICurrencyConversionService _currencyService;

    [HttpPost("{claimNumber}/authorize-payment")]
    public async Task<ActionResult<PaymentAuthorizationResponse>> AuthorizePayment(
        string claimNumber,
        [FromBody] PaymentAuthorizationRequest request)
    {
        // Retrieve claim details
        var claim = await _claimService.GetClaimAsync(claimNumber);

        if (claim == null)
            return NotFound($"Sinistro {claimNumber} não encontrado");

        // Convert payment amount to BTNF if needed
        decimal amountInBtnf = claim.PrincipalValue;

        if (request.ConvertToBtnf)
        {
            try
            {
                amountInBtnf = await _currencyService.ConvertToBtnfAsync(
                    claim.PrincipalValue,
                    request.TransactionDate,
                    claim.BranchCode);
            }
            catch (CurrencyConversionException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Create payment authorization with converted amount
        var authorization = await _claimService.AuthorizePaymentAsync(
            claimNumber,
            amountInBtnf,
            request.AuthorizedBy);

        return Ok(new PaymentAuthorizationResponse
        {
            ClaimNumber = claimNumber,
            OriginalAmount = claim.PrincipalValue,
            AuthorizedAmount = amountInBtnf,
            ExchangeRate = amountInBtnf / claim.PrincipalValue,
            AuthorizationDate = DateTime.Now
        });
    }
}
```

#### 6. Unit Tests

```csharp
public class CurrencyConversionServiceTests
{
    [Fact]
    public async Task ConvertToBtnf_ValidRate_ReturnsCorrectAmount()
    {
        // Arrange
        var mockRepo = new Mock<ICurrencyRepository>();
        mockRepo.Setup(r => r.GetExchangeRateAsync("BTNF", It.IsAny<DateTime>(), 123))
            .ReturnsAsync(new CurrencyUnit
            {
                CurrencyCode = "BTNF",
                ExchangeRate = 2.75m,
                EffectiveStartDate = new DateTime(2025, 1, 1),
                EffectiveEndDate = new DateTime(2025, 12, 31)
            });

        var service = new CurrencyConversionService(
            mockRepo.Object,
            Mock.Of<ILogger<CurrencyConversionService>>());

        // Act
        var result = await service.ConvertToBtnfAsync(
            100.00m,
            new DateTime(2025, 6, 15),
            123);

        // Assert
        Assert.Equal(275.00m, result); // 100 * 2.75 = 275.00
    }

    [Fact]
    public async Task ConvertToBtnf_NoRateFound_ThrowsException()
    {
        // Arrange
        var mockRepo = new Mock<ICurrencyRepository>();
        mockRepo.Setup(r => r.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync((CurrencyUnit?)null);

        var service = new CurrencyConversionService(
            mockRepo.Object,
            Mock.Of<ILogger<CurrencyConversionService>>());

        // Act & Assert
        await Assert.ThrowsAsync<CurrencyConversionException>(
            () => service.ConvertToBtnfAsync(100m, DateTime.Now, 123));
    }

    [Theory]
    [InlineData(100.123, 2.5, 250.31)] // 100.123 * 2.5 = 250.3075 → 250.31 (rounded)
    [InlineData(33.335, 3.0, 100.01)]  // 33.335 * 3.0 = 100.005 → 100.01 (away from zero)
    [InlineData(50.00, 1.0, 50.00)]    // No conversion
    public async Task ConvertToBtnf_RoundsToTwoDecimals(decimal amount, decimal rate, decimal expected)
    {
        // Arrange
        var mockRepo = new Mock<ICurrencyRepository>();
        mockRepo.Setup(r => r.GetExchangeRateAsync("BTNF", It.IsAny<DateTime>(), 1))
            .ReturnsAsync(new CurrencyUnit { ExchangeRate = rate });

        var service = new CurrencyConversionService(
            mockRepo.Object,
            Mock.Of<ILogger<CurrencyConversionService>>());

        // Act
        var result = await service.ConvertToBtnfAsync(amount, DateTime.Now, 1);

        // Assert
        Assert.Equal(expected, result);
    }
}
```

### Risks/Tradeoffs

1. **Missing Exchange Rates**
   - **Risk**: No rate available for transaction date (edge case mentioned in spec)
   - **Mitigation**: Throw clear exception with Portuguese message, log for manual intervention, consider using nearest available rate with warning

2. **Rate Changes During Transaction**
   - **Risk**: Exchange rate changes between claim retrieval and payment authorization
   - **Mitigation**: Use transaction date (not current date) for rate lookup, store rate used in history record

3. **Decimal Precision Overflow**
   - **Risk**: Very large amounts could exceed decimal range (max: ~79 octillion)
   - **Mitigation**: Unlikely for insurance claims (usually < millions), add range validation if needed

4. **Timezone Handling**
   - **Risk**: `DateTime` without timezone could cause date boundary issues
   - **Mitigation**: Use `.Date` property to strip time component, standardize on UTC or São Paulo timezone

**Official References**:
- [Decimal Type Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types#characteristics-of-the-floating-point-types)
- [Math.Round Method](https://learn.microsoft.com/en-us/dotnet/api/system.math.round)
- [Financial Calculations Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)

---

## 5. Dashboard Real-Time Updates

### Decision

**Polling-based updates** using **TanStack Query (React Query) v5** with 30-second automatic refetch interval. Simpler and more reliable than WebSockets for dashboard metrics.

### Rationale

Polling with React Query provides:

- **Simplicity**: No WebSocket server infrastructure needed
- **Browser Compatibility**: Works everywhere (no WS upgrade negotiation)
- **Automatic Caching**: React Query handles cache invalidation and background refetching
- **Stale-While-Revalidate**: Shows cached data immediately, updates in background
- **Error Handling**: Built-in retry logic and error boundaries
- **Bandwidth Efficiency**: Only fetches when tab is visible (automatic pause when inactive)

For a dashboard refreshing every 30 seconds, the overhead of HTTP polling is negligible compared to WebSocket maintenance complexity. This is not a real-time trading system - 30-second delays are acceptable.

### Alternatives Considered

#### Alternative A: SignalR (WebSocket Push)
**Pros**:
- True real-time updates (< 1 second latency)
- Server pushes data to clients (more efficient for frequent updates)
- Built into ASP.NET Core
- Auto-reconnect on connection loss

**Cons**:
- Requires persistent connections (connection pool exhaustion at scale)
- More complex backend implementation (hubs, groups, connection management)
- Firewall/proxy issues with WebSocket upgrade in corporate environments
- Overkill for 30-second refresh requirement (FR-044)
- State synchronization challenges (what if client misses a message?)

**Why Not Chosen**: 30-second refresh doesn't require sub-second latency. Polling is simpler and just as effective.

#### Alternative B: Server-Sent Events (SSE)
**Pros**:
- Simpler than WebSockets (one-way server → client)
- Works over HTTP (no upgrade required)
- Auto-reconnect built-in
- Lighter than SignalR

**Cons**:
- One-way only (can't send commands from client to server)
- Limited browser support on older IE/Edge
- Connection limit in HTTP/1.1 (6 per domain)
- Still requires persistent connections

**Why Not Chosen**: Benefit over polling is minimal for 30s refresh. Polling is more universally compatible.

#### Alternative C: Long Polling
**Pros**:
- Simpler than WebSockets
- Works with any HTTP infrastructure
- Better than regular polling (server holds request until data changes)

**Cons**:
- More complex than short polling (requires server-side async handling)
- Connection timeouts require careful tuning
- Not significantly better than 30s short polling for this use case

**Why Not Chosen**: 30-second polling is already simple and effective. Long polling adds complexity without benefit.

### Implementation Notes

#### 1. Install React Query

```bash
npm install @tanstack/react-query@5.51.0
```

#### 2. Setup Query Client (src/App.tsx)

```typescript
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: true, // Refetch when user returns to tab
      refetchOnReconnect: true, // Refetch when internet reconnects
      retry: 3, // Retry failed requests 3 times
      staleTime: 25000, // Consider data stale after 25 seconds
      gcTime: 300000, // Garbage collect unused queries after 5 minutes
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        {/* Routes */}
      </Router>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
```

#### 3. API Service Layer (src/services/dashboardApi.ts)

```typescript
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001';

export interface DashboardOverview {
  totalComponents: number;
  migratedComponents: number;
  inProgressComponents: number;
  pendingComponents: number;
  overallProgress: number;
  estimatedCompletion: string;
}

export interface PerformanceMetric {
  metricName: string;
  visualAgeValue: number;
  dotNetValue: number;
  improvement: number;
  unit: string;
}

export interface RecentActivity {
  id: number;
  timestamp: string;
  componentName: string;
  action: string;
  status: 'success' | 'in_progress' | 'failed';
  performedBy: string;
}

export const dashboardApi = {
  getOverview: async (): Promise<DashboardOverview> => {
    const response = await axios.get(`${API_BASE_URL}/api/dashboard/overview`);
    return response.data;
  },

  getPerformanceMetrics: async (): Promise<PerformanceMetric[]> => {
    const response = await axios.get(`${API_BASE_URL}/api/dashboard/performance`);
    return response.data;
  },

  getRecentActivities: async (limit: number = 10): Promise<RecentActivity[]> => {
    const response = await axios.get(`${API_BASE_URL}/api/dashboard/activities`, {
      params: { limit },
    });
    return response.data;
  },
};
```

#### 4. Custom Hooks for Dashboard Data

```typescript
// src/hooks/useDashboardData.ts
import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../services/dashboardApi';

export const useDashboardOverview = () => {
  return useQuery({
    queryKey: ['dashboard', 'overview'],
    queryFn: dashboardApi.getOverview,
    refetchInterval: 30000, // Refetch every 30 seconds (FR-044)
    staleTime: 25000, // Data is fresh for 25 seconds
  });
};

export const usePerformanceMetrics = () => {
  return useQuery({
    queryKey: ['dashboard', 'performance'],
    queryFn: dashboardApi.getPerformanceMetrics,
    refetchInterval: 30000,
  });
};

export const useRecentActivities = (limit: number = 10) => {
  return useQuery({
    queryKey: ['dashboard', 'activities', limit],
    queryFn: () => dashboardApi.getRecentActivities(limit),
    refetchInterval: 30000,
  });
};
```

#### 5. Dashboard Page Component

```typescript
// src/pages/MigrationDashboardPage.tsx
import React from 'react';
import {
  useDashboardOverview,
  usePerformanceMetrics,
  useRecentActivities,
} from '../hooks/useDashboardData';
import OverviewCard from '../components/dashboard/OverviewCard';
import PerformanceChart from '../components/dashboard/PerformanceChart';
import ActivitiesTimeline from '../components/dashboard/ActivitiesTimeline';
import LoadingSpinner from '../components/common/LoadingSpinner';

const MigrationDashboardPage: React.FC = () => {
  const { data: overview, isLoading: overviewLoading, error: overviewError } = useDashboardOverview();
  const { data: performance, isLoading: perfLoading } = usePerformanceMetrics();
  const { data: activities, isLoading: activitiesLoading } = useRecentActivities(10);

  if (overviewLoading) {
    return <LoadingSpinner message="Carregando painel de migração..." />;
  }

  if (overviewError) {
    return (
      <div className="error-message">
        Erro ao carregar dados do painel. Por favor, tente novamente.
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <h1>Painel de Migração Visual Age → .NET 9</h1>
        <p className="last-updated">
          Última atualização: {new Date().toLocaleTimeString('pt-BR')}
        </p>
      </header>

      <div className="dashboard-grid">
        {/* Overview Cards */}
        <OverviewCard
          title="Progresso Geral"
          value={`${overview?.overallProgress ?? 0}%`}
          subtitle={`${overview?.migratedComponents ?? 0} de ${overview?.totalComponents ?? 0} componentes`}
        />

        {/* Performance Comparison */}
        {!perfLoading && performance && (
          <PerformanceChart metrics={performance} />
        )}

        {/* Recent Activities */}
        {!activitiesLoading && activities && (
          <ActivitiesTimeline activities={activities} />
        )}
      </div>
    </div>
  );
};

export default MigrationDashboardPage;
```

#### 6. Auto-Refresh Indicator Component

```typescript
// src/components/dashboard/RefreshIndicator.tsx
import React, { useEffect, useState } from 'react';
import { useIsFetching } from '@tanstack/react-query';

const RefreshIndicator: React.FC = () => {
  const isFetching = useIsFetching(); // Global fetching state
  const [countdown, setCountdown] = useState(30);

  useEffect(() => {
    const interval = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          return 30; // Reset countdown
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  return (
    <div className="refresh-indicator">
      {isFetching > 0 ? (
        <span className="fetching">Atualizando dados...</span>
      ) : (
        <span className="idle">
          Próxima atualização em {countdown}s
        </span>
      )}
    </div>
  );
};

export default RefreshIndicator;
```

#### 7. Manual Refresh Button

```typescript
import { useQueryClient } from '@tanstack/react-query';

const RefreshButton: React.FC = () => {
  const queryClient = useQueryClient();

  const handleRefresh = () => {
    // Invalidate all dashboard queries to force refetch
    queryClient.invalidateQueries({ queryKey: ['dashboard'] });
  };

  return (
    <button onClick={handleRefresh} className="refresh-button">
      Atualizar Agora
    </button>
  );
};
```

#### 8. Background Refetch Optimization

```typescript
// Only refetch when browser tab is active
import { useQuery } from '@tanstack/react-query';

export const useDashboardOverview = () => {
  return useQuery({
    queryKey: ['dashboard', 'overview'],
    queryFn: dashboardApi.getOverview,
    refetchInterval: (data, query) => {
      // Pause refetching when tab is hidden
      if (document.hidden) {
        return false; // Pause
      }
      return 30000; // Refetch every 30s when tab is visible
    },
    refetchIntervalInBackground: false, // Don't refetch in background
  });
};
```

### Risks/Tradeoffs

1. **Bandwidth Overhead**
   - **Risk**: 1000 concurrent users polling every 30s = ~33 requests/second
   - **Mitigation**: React Query only polls visible tabs, implement HTTP caching headers (ETag, Last-Modified)

2. **Stale Data During Network Issues**
   - **Risk**: Failed refetch leaves old data on screen
   - **Mitigation**: React Query shows last successful data with error indicator, retry logic

3. **Server Load**
   - **Risk**: Thundering herd problem if all clients refresh at same time
   - **Mitigation**: Add random jitter to refetch interval (± 5 seconds)

4. **Battery Drain on Mobile**
   - **Risk**: Continuous polling drains mobile device battery
   - **Mitigation**: Increase refetch interval on mobile devices (detect via `navigator.userAgent`)

**Official References**:
- [TanStack Query Documentation](https://tanstack.com/query/latest/docs/framework/react/overview)
- [React Query vs SignalR Discussion](https://tkdodo.eu/blog/react-query-and-forms)
- [Polling Best Practices](https://tanstack.com/query/latest/docs/framework/react/guides/window-focus-refetching)

---

## 6. Charting Library Selection

### Decision

**Recharts 2.x** - React-native charting library built on D3 primitives. Provides composable components for progress bars, pie charts, line charts, and bar charts with TypeScript support.

### Rationale

Recharts is optimal for this dashboard because:

- **React-Native**: Uses React components (not canvas), integrates seamlessly with React lifecycle
- **Composable**: Build complex charts from simple components (`<BarChart>`, `<Line>`, `<Pie>`)
- **Responsive**: Automatic sizing with `ResponsiveContainer`
- **TypeScript Support**: Strong typing for props and data structures
- **Customizable**: Easy to apply custom colors (green theme #00A859 from spec)
- **Accessible**: Supports ARIA labels and keyboard navigation
- **Active Maintenance**: Regular updates, large community (22k+ GitHub stars)
- **Small Bundle Size**: ~70KB gzipped (smaller than Chart.js)

The component-based API matches React patterns better than imperative canvas libraries.

### Alternatives Considered

#### Alternative A: Chart.js with react-chartjs-2
**Pros**:
- Most popular charting library (60k+ stars)
- Excellent documentation
- Wide plugin ecosystem
- Good performance with large datasets

**Cons**:
- Canvas-based (harder to integrate with React patterns)
- Imperative API (less React-like)
- react-chartjs-2 wrapper adds complexity
- Harder to customize beyond default options
- Accessibility requires extra work

**Why Not Chosen**: Canvas rendering doesn't align with React's declarative paradigm. Recharts' component model is more maintainable.

#### Alternative B: Nivo
**Pros**:
- Beautiful default designs
- D3-based (very customizable)
- Server-side rendering support
- Rich animation support

**Cons**:
- Larger bundle size (~150KB gzipped)
- Steeper learning curve (more D3 knowledge required)
- Overkill for simple dashboard charts
- Less documentation than Recharts

**Why Not Chosen**: Nivo is powerful but unnecessarily complex for card-based dashboard metrics. Recharts is simpler.

#### Alternative C: Victory (Formidable Labs)
**Pros**:
- Mobile-friendly (supports React Native)
- Component-based like Recharts
- Good animation support
- Strong focus on accessibility

**Cons**:
- Smaller community than Recharts
- Less frequent updates (last major release 2022)
- More verbose API than Recharts
- Limited documentation

**Why Not Chosen**: Recharts has better community support and clearer documentation. Victory is less actively maintained.

### Implementation Notes

#### 1. Installation

```bash
npm install recharts@2.12.0
npm install --save-dev @types/recharts
```

#### 2. Dashboard Overview Card (Progress Bar)

```typescript
// src/components/dashboard/ProgressCard.tsx
import React from 'react';
import { BarChart, Bar, Cell, XAxis, YAxis, ResponsiveContainer } from 'recharts';

interface ProgressCardProps {
  title: string;
  current: number;
  total: number;
}

const ProgressCard: React.FC<ProgressCardProps> = ({ title, current, total }) => {
  const percentage = Math.round((current / total) * 100);

  const data = [
    { name: 'Concluído', value: current, color: '#00A859' },
    { name: 'Pendente', value: total - current, color: '#E0E0E0' },
  ];

  return (
    <div className="progress-card">
      <h3>{title}</h3>
      <div className="progress-value">
        {current} / {total} ({percentage}%)
      </div>

      <ResponsiveContainer width="100%" height={60}>
        <BarChart data={data} layout="vertical">
          <XAxis type="number" hide />
          <YAxis type="category" hide />
          <Bar dataKey="value" stackId="a">
            {data.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export default ProgressCard;
```

#### 3. Migration Status Pie Chart

```typescript
// src/components/dashboard/MigrationStatusPieChart.tsx
import React from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';

interface MigrationStatusData {
  migrated: number;
  inProgress: number;
  pending: number;
}

const MigrationStatusPieChart: React.FC<{ data: MigrationStatusData }> = ({ data }) => {
  const chartData = [
    { name: 'Migrados', value: data.migrated, color: '#00A859' },
    { name: 'Em Progresso', value: data.inProgress, color: '#FFA500' },
    { name: 'Pendentes', value: data.pending, color: '#E0E0E0' },
  ];

  return (
    <div className="chart-card">
      <h3>Status de Migração</h3>
      <ResponsiveContainer width="100%" height={300}>
        <PieChart>
          <Pie
            data={chartData}
            dataKey="value"
            nameKey="name"
            cx="50%"
            cy="50%"
            outerRadius={80}
            label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
          >
            {chartData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} />
            ))}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
};

export default MigrationStatusPieChart;
```

#### 4. Performance Comparison Bar Chart

```typescript
// src/components/dashboard/PerformanceChart.tsx
import React from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

interface PerformanceMetric {
  metricName: string;
  visualAgeValue: number;
  dotNetValue: number;
  unit: string;
}

const PerformanceChart: React.FC<{ metrics: PerformanceMetric[] }> = ({ metrics }) => {
  const data = metrics.map((m) => ({
    name: m.metricName,
    'Visual Age': m.visualAgeValue,
    '.NET 9': m.dotNetValue,
  }));

  return (
    <div className="chart-card">
      <h3>Comparação de Performance</h3>
      <ResponsiveContainer width="100%" height={400}>
        <BarChart data={data} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
          <YAxis label={{ value: 'Tempo (ms)', angle: -90, position: 'insideLeft' }} />
          <Tooltip />
          <Legend />
          <Bar dataKey="Visual Age" fill="#8884d8" />
          <Bar dataKey=".NET 9" fill="#00A859" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export default PerformanceChart;
```

#### 5. Timeline Line Chart (Migration Progress Over Time)

```typescript
// src/components/dashboard/MigrationTimelineChart.tsx
import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';

interface TimelineData {
  date: string;
  migratedComponents: number;
}

const MigrationTimelineChart: React.FC<{ data: TimelineData[] }> = ({ data }) => {
  return (
    <div className="chart-card">
      <h3>Progresso ao Longo do Tempo</h3>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="date" />
          <YAxis label={{ value: 'Componentes', angle: -90, position: 'insideLeft' }} />
          <Tooltip />
          <Line
            type="monotone"
            dataKey="migratedComponents"
            stroke="#00A859"
            strokeWidth={2}
            dot={{ fill: '#00A859' }}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};

export default MigrationTimelineChart;
```

#### 6. Custom Tooltip

```typescript
const CustomTooltip: React.FC<any> = ({ active, payload, label }) => {
  if (active && payload && payload.length) {
    return (
      <div className="custom-tooltip">
        <p className="label">{label}</p>
        {payload.map((entry: any, index: number) => (
          <p key={index} style={{ color: entry.color }}>
            {entry.name}: {entry.value} {entry.unit || ''}
          </p>
        ))}
      </div>
    );
  }
  return null;
};

// Use in chart
<Tooltip content={<CustomTooltip />} />
```

#### 7. Responsive Design

```typescript
// Adjust chart height based on screen size
import { useMediaQuery } from 'react-responsive';

const DashboardChart: React.FC = () => {
  const isMobile = useMediaQuery({ maxWidth: 850 }); // SC-010 mobile breakpoint

  const chartHeight = isMobile ? 250 : 400;

  return (
    <ResponsiveContainer width="100%" height={chartHeight}>
      {/* Chart components */}
    </ResponsiveContainer>
  );
};
```

#### 8. Accessibility

```typescript
<ResponsiveContainer
  width="100%"
  height={300}
  aria-label="Gráfico de progresso de migração"
  role="img"
>
  <BarChart data={data}>
    {/* Chart configuration */}
  </BarChart>
</ResponsiveContainer>

// Add text description for screen readers
<div className="sr-only">
  {`${migratedComponents} de ${totalComponents} componentes foram migrados,
   representando ${percentage}% do total.`}
</div>
```

### Risks/Tradeoffs

1. **Large Dataset Performance**
   - **Risk**: Rendering hundreds of data points in charts could slow down rendering
   - **Mitigation**: Paginate or aggregate data server-side, limit chart points to 50-100

2. **SVG Accessibility**
   - **Risk**: Recharts uses SVG which screen readers may not fully interpret
   - **Mitigation**: Add ARIA labels, provide text summaries alongside charts

3. **Mobile Responsiveness**
   - **Risk**: Complex charts may be hard to read on small screens
   - **Mitigation**: Simplify charts on mobile (fewer data points), use horizontal scrolling if needed

4. **Color Blindness**
   - **Risk**: Green/red color scheme may be indistinguishable for color-blind users
   - **Mitigation**: Use patterns or shapes in addition to colors, test with color blindness simulator

**Official References**:
- [Recharts Documentation](https://recharts.org/)
- [Recharts Examples](https://recharts.org/en-US/examples)
- [Accessibility in Data Visualization](https://www.w3.org/WAI/tutorials/images/complex/)

---

## 7. Testing Strategy

### Decision

**Multi-layered testing approach**:
1. **Unit Tests**: xUnit + Moq for business logic (Core layer)
2. **Integration Tests**: WebApplicationFactory for API endpoints with in-memory database
3. **Contract Tests**: Verify external service integrations (CNOUA, SIPUA, SIMDA)
4. **Frontend Tests**: Jest + React Testing Library for components
5. **E2E Tests**: Playwright for critical user journeys
6. **Parity Tests**: Snapshot testing comparing Visual Age outputs with .NET outputs

### Rationale

100% functional parity (SC-004) requires comprehensive testing at every layer:

- **Unit Tests**: Verify 42 business rules in isolation (fast, deterministic)
- **Integration Tests**: Ensure API contracts work end-to-end with database
- **Contract Tests**: Validate assumptions about external services
- **Frontend Tests**: Confirm UI behavior without manual testing
- **E2E Tests**: Validate complete user flows (search → authorize payment)
- **Parity Tests**: Prove equivalence between legacy and new system

This strategy balances speed (unit tests run in seconds) with confidence (E2E tests catch integration issues).

### Alternatives Considered

#### Alternative A: Manual Testing Only
**Pros**:
- No test code to maintain
- Faster initial development

**Cons**:
- Regression risk with every change
- Cannot prove 100% parity (SC-004)
- Time-consuming for 55 functional requirements
- Not repeatable or scalable

**Why Not Chosen**: Manual testing cannot guarantee functional parity. Automated tests are essential.

#### Alternative B: E2E Tests Only
**Pros**:
- Tests from user perspective
- Catches integration issues

**Cons**:
- Slow (minutes to run full suite)
- Brittle (UI changes break tests)
- Hard to debug failures (many layers involved)
- Doesn't test business logic in isolation

**Why Not Chosen**: E2E tests alone are too slow for TDD workflow. Unit tests provide fast feedback.

#### Alternative C: 100% Code Coverage Target
**Pros**:
- High confidence in test coverage

**Cons**:
- Diminishing returns (testing getters/setters adds little value)
- Can encourage testing implementation instead of behavior
- Time-consuming to achieve 100%

**Why Not Chosen**: Focus on behavior coverage, not code coverage. Aim for 80-90% coverage with emphasis on critical paths.

### Implementation Notes

#### 1. Backend Testing Packages

```xml
<!-- CaixaSeguradora.Core.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="xunit" Version="2.9.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.0" />
  <PackageReference Include="Moq" Version="4.20.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
</ItemGroup>

<!-- CaixaSeguradora.Api.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
</ItemGroup>
```

#### 2. Unit Test Example (Business Logic)

```csharp
// CaixaSeguradora.Core.Tests/Services/ClaimServiceTests.cs
using Xunit;
using Moq;
using FluentAssertions;

public class ClaimServiceTests
{
    private readonly Mock<IClaimRepository> _mockRepo;
    private readonly Mock<ICurrencyConversionService> _mockCurrency;
    private readonly Mock<ILogger<ClaimService>> _mockLogger;
    private readonly ClaimService _service;

    public ClaimServiceTests()
    {
        _mockRepo = new Mock<IClaimRepository>();
        _mockCurrency = new Mock<ICurrencyConversionService>();
        _mockLogger = new Mock<ILogger<ClaimService>>();
        _service = new ClaimService(_mockRepo.Object, _mockCurrency.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AuthorizePayment_ValidClaim_CreatesHistoryRecord()
    {
        // Arrange
        var claim = new ClaimMaster
        {
            NumSinistro = "12345",
            PrincipalValue = 1000m,
            Status = "PENDING"
        };

        _mockRepo.Setup(r => r.GetByClaimNumberAsync("12345"))
            .ReturnsAsync(claim);

        // Act
        var result = await _service.AuthorizePaymentAsync("12345", "operator123");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("AUTHORIZED");

        _mockRepo.Verify(r => r.AddHistoryAsync(It.Is<ClaimHistory>(h =>
            h.NumSinistro == "12345" &&
            h.Action == "PAYMENT_AUTH" &&
            h.UserID == "operator123"
        )), Times.Once);
    }

    [Theory]
    [InlineData(6814)] // Consórcio Contemplado
    [InlineData(7701)] // Consórcio Desistente
    [InlineData(7709)] // Consórcio Excluído
    public async Task ValidateConsortiumProduct_SpecialProducts_CallsSimdaValidation(int productCode)
    {
        // Arrange
        var claim = new ClaimMaster { ProductCode = productCode };

        // Act
        var result = await _service.ValidateClaimAsync(claim);

        // Assert
        // Verify SIMDA validation was called (FR-028, FR-029)
        _mockRepo.Verify(r => r.ValidateConsortiumAsync(It.IsAny<string>()), Times.Once);
    }
}
```

#### 3. Integration Test Example (API + Database)

```csharp
// CaixaSeguradora.Api.Tests/Controllers/ClaimsControllerTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ClaimsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ClaimsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real database with in-memory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ClaimsDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ClaimsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
                SeedTestData(db);
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SearchClaims_ByProtocol_ReturnsMatchingClaims()
    {
        // Arrange
        var request = new { protocol = "PROT12345" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/claims/search", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var claims = await response.Content.ReadFromJsonAsync<List<ClaimDto>>();
        claims.Should().NotBeEmpty();
        claims.Should().AllSatisfy(c => c.ProtocolNumber.Should().Be("PROT12345"));
    }

    [Fact]
    public async Task AuthorizePayment_ValidRequest_Returns200AndUpdatesDatabase()
    {
        // Arrange
        var request = new PaymentAuthorizationRequest
        {
            ClaimNumber = "SIN00001",
            AuthorizedBy = "operator123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/claims/SIN00001/authorize-payment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database update
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var claim = await db.ClaimMasters.FindAsync("SIN00001");
        claim.Status.Should().Be("AUTHORIZED");
    }

    private void SeedTestData(ClaimsDbContext db)
    {
        db.ClaimMasters.Add(new ClaimMaster
        {
            NumSinistro = "SIN00001",
            ProtocolNumber = "PROT12345",
            PrincipalValue = 5000m,
            Status = "PENDING"
        });
        db.SaveChanges();
    }
}
```

#### 4. Frontend Testing Setup

```bash
# Install testing libraries
npm install --save-dev @testing-library/react@16.0.0
npm install --save-dev @testing-library/jest-dom@6.4.0
npm install --save-dev @testing-library/user-event@14.5.0
npm install --save-dev jest@29.7.0
npm install --save-dev jest-environment-jsdom@29.7.0
```

#### 5. React Component Test Example

```typescript
// frontend/tests/components/ClaimSearchPage.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import ClaimSearchPage from '../../src/pages/ClaimSearchPage';
import * as claimsApi from '../../src/services/claimsApi';

jest.mock('../../src/services/claimsApi');

describe('ClaimSearchPage', () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );

  it('searches claims by protocol number', async () => {
    // Arrange
    const mockClaims = [
      { claimNumber: 'SIN001', protocolNumber: 'PROT123', principalValue: 1000 },
    ];

    (claimsApi.searchClaims as jest.Mock).mockResolvedValue(mockClaims);

    render(<ClaimSearchPage />, { wrapper });

    // Act
    const protocolInput = screen.getByLabelText(/Protocolo/i);
    const searchButton = screen.getByRole('button', { name: /Pesquisar/i });

    await userEvent.type(protocolInput, 'PROT123');
    await userEvent.click(searchButton);

    // Assert
    await waitFor(() => {
      expect(screen.getByText('SIN001')).toBeInTheDocument();
    });

    expect(claimsApi.searchClaims).toHaveBeenCalledWith({ protocol: 'PROT123' });
  });

  it('displays error message when search fails', async () => {
    // Arrange
    (claimsApi.searchClaims as jest.Mock).mockRejectedValue(
      new Error('Erro ao pesquisar sinistros')
    );

    render(<ClaimSearchPage />, { wrapper });

    // Act
    const searchButton = screen.getByRole('button', { name: /Pesquisar/i });
    await userEvent.click(searchButton);

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/Erro ao pesquisar/i)).toBeInTheDocument();
    });
  });
});
```

#### 6. E2E Test with Playwright

```bash
npm install --save-dev @playwright/test@1.45.0
npx playwright install
```

```typescript
// frontend/tests/e2e/claim-authorization.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Payment Authorization Flow', () => {
  test('operator can search claim and authorize payment', async ({ page }) => {
    // Navigate to search page
    await page.goto('http://localhost:3000/claims/search');

    // Search for claim
    await page.fill('input[name="claimNumber"]', 'SIN00001');
    await page.click('button:has-text("Pesquisar")');

    // Wait for results
    await expect(page.locator('text=SIN00001')).toBeVisible();

    // Click to view details
    await page.click('text=SIN00001');

    // Authorize payment
    await expect(page.locator('h2:has-text("Detalhes do Sinistro")')).toBeVisible();
    await page.click('button:has-text("Autorizar Pagamento")');

    // Fill authorization form
    await page.fill('input[name="authorizedBy"]', 'operator123');
    await page.click('button:has-text("Confirmar")');

    // Verify success message
    await expect(page.locator('text=Pagamento autorizado com sucesso')).toBeVisible();

    // Verify status updated
    await expect(page.locator('text=Status: AUTORIZADO')).toBeVisible();
  });

  test('system prevents rollback after external validation', async ({ page }) => {
    // Setup: Create claim with external validation complete
    await page.goto('http://localhost:3000/claims/SIN00002');

    // Try to edit claim after validation
    await page.click('button:has-text("Editar")');

    // Verify error message (FR-030)
    await expect(page.locator(
      'text=Não é possível alterar sinistro após validação externa'
    )).toBeVisible();
  });
});
```

#### 7. Parity Testing (Visual Age vs .NET)

```csharp
// CaixaSeguradora.Core.Tests/ParityTests/CurrencyConversionParityTests.cs
public class CurrencyConversionParityTests
{
    [Theory]
    [MemberData(nameof(VisualAgeTestCases))]
    public async Task ConvertToBtnf_MatchesVisualAgeOutput(
        decimal amount,
        DateTime date,
        int branchCode,
        decimal expectedVisualAgeResult)
    {
        // Arrange
        var service = CreateCurrencyConversionService();

        // Act
        var dotNetResult = await service.ConvertToBtnfAsync(amount, date, branchCode);

        // Assert - Must match Visual Age exactly
        dotNetResult.Should().Be(expectedVisualAgeResult);
    }

    public static IEnumerable<object[]> VisualAgeTestCases()
    {
        // Test cases extracted from Visual Age system
        // Format: amount, date, branchCode, expectedResult
        yield return new object[] { 1000m, new DateTime(2025, 1, 15), 1, 2750.00m };
        yield return new object[] { 500.50m, new DateTime(2025, 2, 20), 1, 1376.38m };
        // ... more cases from Visual Age test outputs
    }
}
```

#### 8. Test Data Management

```csharp
public class TestDataBuilder
{
    public static ClaimMaster CreateValidClaim(string claimNumber = "TEST001")
    {
        return new ClaimMaster
        {
            NumSinistro = claimNumber,
            ProtocolNumber = $"PROT{claimNumber}",
            PrincipalValue = 1000m,
            BranchCode = 1,
            Status = "PENDING",
            CreatedDate = DateTime.Now,
            LastModifiedBy = "test_user"
        };
    }

    public static ClaimHistory CreateHistory(string claimNumber, string action)
    {
        return new ClaimHistory
        {
            NumSinistro = claimNumber,
            DateMovement = DateTime.Now,
            Action = action,
            UserID = "test_operator",
            Details = $"Test {action}"
        };
    }
}
```

### Risks/Tradeoffs

1. **Test Maintenance Overhead**
   - **Risk**: Large test suites become slow and brittle
   - **Mitigation**: Focus on behavior over implementation, use Page Object Model for E2E tests

2. **Parity Test Data Availability**
   - **Risk**: May not have access to Visual Age system to extract test cases
   - **Mitigation**: Work with Visual Age operators to record input/output pairs, use production logs if available

3. **Flaky E2E Tests**
   - **Risk**: UI tests fail intermittently due to timing issues
   - **Mitigation**: Use Playwright's auto-wait, avoid hardcoded sleeps, retry flaky tests

4. **Test Environment Costs**
   - **Risk**: Running full test suite on every commit is slow (CI/CD bottleneck)
   - **Mitigation**: Parallelize tests, run unit tests on every commit, E2E tests nightly or pre-merge

**Official References**:
- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [Playwright Documentation](https://playwright.dev/)

---

## 8. Authentication & Authorization

### Decision

**JWT-based authentication** with **Active Directory/Azure AD integration** for operator identity. Maps EZEUSRID to AD user principal, stores JWT in httpOnly cookies for security.

### Rationale

JWT + Active Directory aligns with enterprise security standards:

- **Single Sign-On (SSO)**: Operators use existing corporate credentials (no new passwords)
- **Centralized User Management**: IT manages user accounts in AD (provisioning/deprovisioning)
- **JWT Tokens**: Stateless authentication (no server-side session storage, scales horizontally)
- **httpOnly Cookies**: Prevents XSS attacks (JavaScript cannot access token)
- **Role-Based Access Control**: Map AD groups to application roles if needed
- **Audit Trail**: Preserve EZEUSRID in all database records (FR-051, FR-052)

This approach maintains the user audit trail requirement while providing modern security.

### Alternatives Considered

#### Alternative A: Session-Based Authentication (Legacy Style)
**Pros**:
- Simpler to implement (built into ASP.NET Core)
- Familiar pattern from Visual Age
- Easy to invalidate sessions (logout)

**Cons**:
- Requires server-side session storage (Redis, database)
- Doesn't scale horizontally (sticky sessions needed)
- No standardized format (unlike JWT)
- Session cookies vulnerable to CSRF without additional protection

**Why Not Chosen**: JWT is stateless and scales better for cloud deployments. Industry standard for modern APIs.

#### Alternative B: OAuth2 with Salesforce Identity
**Pros**:
- Leverages existing Salesforce integration
- Standardized protocol
- Third-party identity provider

**Cons**:
- Adds dependency on external service for authentication
- More complex flow (authorization code grant)
- May not align with Caixa Seguradora's identity provider
- Overkill if AD is already available

**Why Not Chosen**: Salesforce is for CRM integration, not user authentication. AD is likely the corporate identity provider.

#### Alternative C: API Keys (Service-to-Service)
**Pros**:
- Simple for service accounts
- No expiration management
- Easy to rotate

**Cons**:
- Not suitable for user authentication (no user identity)
- No audit trail of individual operators
- Hard to revoke individual keys
- Security risk if leaked

**Why Not Chosen**: Requirement FR-051/FR-052 mandates user-level audit trail (EZEUSRID). API keys don't identify users.

### Implementation Notes

#### 1. NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.1.0" />
<PackageReference Include="Microsoft.Identity.Web" Version="3.2.0" /> <!-- For Azure AD -->
```

#### 2. JWT Configuration (appsettings.json)

```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "https://caixaseguradora.com.br",
      "Audience": "claims-api",
      "SecretKey": "YourSuperSecretKeyHere_AtLeast32Characters",
      "ExpirationMinutes": 480
    },
    "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "CallbackPath": "/signin-oidc"
    }
  }
}
```

#### 3. JWT Service Implementation

```csharp
public interface IJwtService
{
    string GenerateToken(string userId, string userName, IEnumerable<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(string userId, string userName, IEnumerable<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Authentication:Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId), // Maps to EZEUSRID
            new Claim(ClaimTypes.Name, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("Authentication:Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
            }, out var validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}
```

#### 4. Active Directory Authentication Service

```csharp
public interface IActiveDirectoryService
{
    Task<(bool Success, string UserId, string UserName, IEnumerable<string> Roles)> AuthenticateAsync(
        string username,
        string password);
}

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public async Task<(bool Success, string UserId, string UserName, IEnumerable<string> Roles)>
        AuthenticateAsync(string username, string password)
    {
        try
        {
            var domain = _configuration["Authentication:ActiveDirectory:Domain"];
            var ldapPath = _configuration["Authentication:ActiveDirectory:LdapPath"];

            // Validate credentials against Active Directory
            using var context = new PrincipalContext(ContextType.Domain, domain);

            if (!context.ValidateCredentials(username, password))
            {
                _logger.LogWarning("Authentication failed for user {Username}", username);
                return (false, string.Empty, string.Empty, Array.Empty<string>());
            }

            // Retrieve user details
            using var userPrincipal = UserPrincipal.FindByIdentity(context, username);

            if (userPrincipal == null)
            {
                _logger.LogWarning("User {Username} not found in AD", username);
                return (false, string.Empty, string.Empty, Array.Empty<string>());
            }

            var userId = userPrincipal.SamAccountName; // Maps to EZEUSRID
            var userName = userPrincipal.DisplayName ?? username;

            // Get user groups (for role-based access control)
            var roles = userPrincipal.GetAuthorizationGroups()
                .Select(g => g.Name)
                .Where(name => name.StartsWith("Claims_")) // Only app-specific groups
                .ToList();

            _logger.LogInformation("User {UserId} authenticated successfully", userId);

            return (true, userId, userName, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AD authentication error for user {Username}", username);
            return (false, string.Empty, string.Empty, Array.Empty<string>());
        }
    }
}
```

#### 5. Login Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IActiveDirectoryService adService,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _adService = adService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Validate credentials against Active Directory
        var (success, userId, userName, roles) = await _adService.AuthenticateAsync(
            request.Username,
            request.Password);

        if (!success)
        {
            _logger.LogWarning("Login failed for user {Username}", request.Username);
            return Unauthorized(new { message = "Credenciais inválidas" });
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(userId, userName, roles);

        // Set token in httpOnly cookie (prevents XSS)
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // HTTPS only
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(480) // 8 hours
        });

        _logger.LogInformation("User {UserId} logged in successfully", userId);

        return Ok(new LoginResponse
        {
            UserId = userId,
            UserName = userName,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(480)
        });
    }

    [HttpPost("logout")]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok(new { message = "Logout realizado com sucesso" });
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        return Ok(new UserInfo
        {
            UserId = userId!,
            UserName = userName!,
            Roles = roles.ToList()
        });
    }
}

public record LoginRequest(string Username, string Password);
public record LoginResponse
{
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public DateTime ExpiresAt { get; init; }
}
```

#### 6. Program.cs JWT Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Read token from cookie (fallback to Authorization header)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("auth_token"))
            {
                context.Token = context.Request.Cookies["auth_token"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

#### 7. Protecting API Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class ClaimsController : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<ClaimDto>>> SearchClaims(
        [FromBody] ClaimSearchRequest request)
    {
        // Get current user ID (EZEUSRID)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogInformation("User {UserId} searching claims", userId);

        // ... search logic
    }

    [HttpPost("{claimNumber}/authorize-payment")]
    [Authorize(Roles = "Claims_Approver")] // Role-based authorization
    public async Task<ActionResult<PaymentAuthorizationResponse>> AuthorizePayment(
        string claimNumber,
        [FromBody] PaymentAuthorizationRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Pass userId to service for audit trail
        var result = await _claimService.AuthorizePaymentAsync(claimNumber, userId!);

        return Ok(result);
    }
}
```

#### 8. Frontend Integration (React)

```typescript
// src/services/authService.ts
import axios from 'axios';

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface UserInfo {
  userId: string;
  userName: string;
  roles: string[];
}

export const authService = {
  login: async (credentials: LoginCredentials): Promise<UserInfo> => {
    const response = await axios.post('/api/auth/login', credentials, {
      withCredentials: true, // Include cookies
    });
    return response.data;
  },

  logout: async (): Promise<void> => {
    await axios.post('/api/auth/logout', {}, { withCredentials: true });
  },

  getCurrentUser: async (): Promise<UserInfo> => {
    const response = await axios.get('/api/auth/me', {
      withCredentials: true,
    });
    return response.data;
  },
};

// Configure axios to always include credentials
axios.defaults.withCredentials = true;
```

```typescript
// src/contexts/AuthContext.tsx
import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService, UserInfo } from '../services/authService';

interface AuthContextType {
  user: UserInfo | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null);

  useEffect(() => {
    // Check if user is already authenticated
    authService.getCurrentUser()
      .then(setUser)
      .catch(() => setUser(null));
  }, []);

  const login = async (username: string, password: string) => {
    const userData = await authService.login({ username, password });
    setUser(userData);
  };

  const logout = async () => {
    await authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{
      user,
      login,
      logout,
      isAuthenticated: user !== null,
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

### Risks/Tradeoffs

1. **Secret Key Management**
   - **Risk**: JWT secret key in appsettings.json is a security vulnerability
   - **Mitigation**: Store in Azure Key Vault, rotate keys periodically, use certificate-based signing in production

2. **Token Expiration UX**
   - **Risk**: Users lose work when token expires mid-operation
   - **Mitigation**: Implement refresh token flow, warn users before expiration, auto-save forms

3. **Active Directory Dependency**
   - **Risk**: Application cannot authenticate if AD is unavailable
   - **Mitigation**: Implement fallback mechanism, consider local admin account for emergencies

4. **XSS/CSRF Attacks**
   - **Risk**: httpOnly cookies protect against XSS but are vulnerable to CSRF
   - **Mitigation**: Use SameSite=Strict attribute, implement CSRF tokens for state-changing operations

**Official References**:
- [JWT.io](https://jwt.io/)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

## Summary of Decisions

| Research Area | Decision | Key Technology |
|--------------|----------|----------------|
| Database Access | Entity Framework Core 9 database-first | IBM.Data.DB2.Core / Microsoft.Data.SqlClient |
| SOAP Endpoints | SoapCore 1.x integrated with ASP.NET Core | SoapCore library |
| External Services | HttpClient with Polly resilience policies | Polly v8 |
| Currency Conversion | Domain service with decimal precision | Built-in decimal type |
| Dashboard Updates | Polling with 30s interval | TanStack Query (React Query) v5 |
| Charting | React-native composable charts | Recharts 2.x |
| Testing | Multi-layered: unit, integration, E2E, parity | xUnit, Moq, Playwright, React Testing Library |
| Authentication | JWT tokens with Active Directory integration | Microsoft.AspNetCore.Authentication.JwtBearer |

## Next Steps

1. **Review & Approval**: Share this research document with technical leads and stakeholders
2. **Proceed to Phase 1**: Create `data-model.md`, `contracts/`, and `quickstart.md`
3. **Update Agent Context**: Run `.specify/scripts/bash/update-agent-context.sh claude`
4. **Prototype Key Decisions**: Build proof-of-concept for highest-risk areas (DB2 connection, SOAP endpoints, external service integration)
5. **Generate Tasks**: Run `/speckit.tasks` to create detailed implementation roadmap

## Document Metadata

- **Created**: 2025-10-23
- **Author**: Claude Code (Anthropic)
- **Version**: 1.0
- **Status**: Complete
- **Review Status**: Pending stakeholder approval
