using CaixaSeguradora.Api.Middleware;
using CaixaSeguradora.Api.SoapServices;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Core.Services;
using CaixaSeguradora.Core.Validators;
using CaixaSeguradora.Infrastructure.Data;
using CaixaSeguradora.Infrastructure.Repositories;
using CaixaSeguradora.Infrastructure.Services;
using CaixaSeguradora.Infrastructure.ExternalServices;
using CaixaSeguradora.Infrastructure.HealthChecks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using SoapCore;
using System.Reflection;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/claims-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Caixa Seguradora Claims API",
        Version = "v1",
        Description = "REST and SOAP API for Claims Management System migrated from IBM Visual Age",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Caixa Seguradora",
            Email = "support@caixaseguradora.com.br"
        }
    });

    // Include XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("ClaimsDatabase")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDatabase' not found.");

builder.Services.AddDbContext<ClaimsDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ClaimSearchValidator>();

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business Services
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IPaymentAuthorizationService, PaymentAuthorizationService>();
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPhaseManagementService, PhaseManagementService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ILegacySystemDocsService, LegacySystemDocsService>();

// T032: Transaction Coordinator for atomic transaction management
builder.Services.AddScoped<TransactionCoordinator>();

// TODO: FASE 1 - Re-enable SOAP Services after fixing property mappings
// SOAP Services
// builder.Services.AddScoped<ISolicitacaoService, SolicitacaoService>();
// builder.Services.AddSoapCore();

// T021: External Validation Services with dedicated interfaces
builder.Services.AddScoped<ICnouaValidationClient, CnouaValidationClient>();
builder.Services.AddScoped<ISipuaValidationClient, SipuaValidationClient>();
builder.Services.AddScoped<ISimdaValidationClient, SimdaValidationClient>();

// T022: External Service Router for routing validation requests
builder.Services.AddScoped<ExternalServiceRouter>();

// Legacy external validation service interface (backwards compatibility)
// Note: Old CNOUA/SIPUA/SIMDA class names removed - use new Cnoua/Sipua/Simda clients instead

// HTTP Client Factory
builder.Services.AddHttpClient();

// T018-T020: Polly resilience policies implemented in validation clients
// - Retry: 3 attempts with exponential backoff (2s, 4s, 8s)
// - Circuit Breaker: Open after 5 failures for 30 seconds
// - Timeout: 10 seconds per request

// HTTP Client for CNOUA REST API
builder.Services.AddHttpClient("CNOUA", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:CNOUA:BaseUrl"] ?? "https://cnoua-service.caixaseguradora.com.br");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "CaixaSeguradora-Claims-API/1.0");
});

// HTTP Client for SIPUA SOAP Service
builder.Services.AddHttpClient("SIPUA", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:SIPUA:BaseUrl"] ?? "https://sipua-service.caixaseguradora.com.br");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "text/xml");
    client.DefaultRequestHeaders.Add("SOAPAction", "http://caixaseguradora.com.br/sipua/ValidateContract");
});

// HTTP Client for SIMDA SOAP Service
builder.Services.AddHttpClient("SIMDA", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:SIMDA:BaseUrl"] ?? "https://simda-service.caixaseguradora.com.br");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "text/xml");
    client.DefaultRequestHeaders.Add("SOAPAction", "http://caixaseguradora.com.br/simda/ValidateHBContract");
});

Log.Information("HTTP clients configured for external services (CNOUA, SIPUA, SIMDA)");

// HTTP Context Accessor (for audit fields)
builder.Services.AddHttpContextAccessor();

// T137 [Security]: JWT Authentication Configuration
var jwtSecretKey = builder.Configuration["JWT:SecretKey"]
    ?? throw new InvalidOperationException("JWT:SecretKey not configured in appsettings");
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "CaixaSeguradoraAPI";
var jwtAudience = builder.Configuration["JWT:Audience"] ?? "CaixaSeguradoraClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Log.Debug("JWT token validated for user: {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// T138 [Security]: CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite default port
                "https://localhost:5173",
                "http://localhost:3000",
                "https://localhost:3000",
                builder.Configuration["FrontendUrl"] ?? "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Authorization", "Content-Disposition");
    });
});

// Response Caching
builder.Services.AddResponseCaching();

// Memory Cache
builder.Services.AddMemoryCache();

// T139 [Performance]: IP Rate Limiting Configuration
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ClaimsDbContext>("database")
    .AddCheck<ExternalServiceHealthCheck>("external_services");

var app = builder.Build();

// Configure the HTTP request pipeline

// Global Exception Handler (must be first)
app.UseGlobalExceptionHandler();

// Serilog Request Logging
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// CORS must be before Authorization
app.UseCors();

// T139 [Performance]: IP Rate Limiting (must be after UseCors)
app.UseIpRateLimiting();

// Response Caching
app.UseResponseCaching();

// T137, T138 [Security]: Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// TODO: FASE 1 - Re-enable SOAP endpoint after fixing SolicitacaoService property mappings
// SOAP Endpoint Configuration
// app.UseSoapEndpoint<ISolicitacaoService>(
//     path: "/soap/solicitacao",
//     encoder: new SoapEncoderOptions
//     {
//         WriteEncoding = System.Text.Encoding.UTF8
//     },
//     serializer: SoapSerializer.XmlSerializer);
//
// Log.Information("SOAP Endpoint registered at /soap/solicitacao (WSDL: /soap/solicitacao?wsdl)");

// Health Check Endpoint
app.MapHealthChecks("/health");

// API Info Endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "Caixa Seguradora Claims API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    endpoints = new
    {
        swagger = "/swagger",
        health = "/health",
        claims = "/api/claims"
    }
}))
.WithName("GetApiInfo")
.ExcludeFromDescription();

try
{
    Log.Information("Starting Caixa Seguradora Claims API");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Database: {ConnectionString}", connectionString.Split(';')[0]);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible to integration tests
public partial class Program { }
