# Security Audit Checklist

**Project**: SIWEA Visual Age Migration to .NET 9.0
**Last Updated**: 2025-10-27
**Purpose**: Comprehensive security audit checklist for production readiness

---

## Table of Contents

1. [SQL Injection Prevention](#1-sql-injection-prevention)
2. [Cross-Site Scripting (XSS) Protection](#2-cross-site-scripting-xss-protection)
3. [Cross-Site Request Forgery (CSRF) Protection](#3-cross-site-request-forgery-csrf-protection)
4. [Sensitive Data Exposure](#4-sensitive-data-exposure)
5. [Insecure Dependencies](#5-insecure-dependencies)
6. [HTTPS Enforcement](#6-https-enforcement)
7. [JWT Security](#7-jwt-security)
8. [CORS Configuration](#8-cors-configuration)
9. [Rate Limiting](#9-rate-limiting)
10. [Additional Security Hardening](#10-additional-security-hardening)

---

## 1. SQL Injection Prevention

### Status: ✅ IMPLEMENTED (Entity Framework Core)

### Verification Steps

- [x] All database queries use Entity Framework Core with parameterized queries
- [x] No raw SQL queries with string concatenation
- [x] Dynamic LINQ queries properly validated

### Implementation Details

**Entity Framework Core** automatically parameterizes all queries, preventing SQL injection:

```csharp
// SAFE - EF Core parameterizes automatically
var claim = await _dbContext.ClaimMaster
    .Where(c => c.NumSinistro == claimNumber && c.OrgSin == branch)
    .FirstOrDefaultAsync();
```

**Raw SQL (if needed)** must use parameters:

```csharp
// SAFE - Using parameterized raw SQL
var results = await _dbContext.ClaimMaster
    .FromSqlRaw("SELECT * FROM TMESTSIN WHERE NUMSIN = {0} AND ORGSIN = {1}",
        claimNumber, branch)
    .ToListAsync();
```

### ❌ AVOID

Never use string concatenation or interpolation for SQL queries as this creates SQL injection vulnerabilities.

### Testing

```bash
# Test with malicious input
curl -X GET "https://localhost:5001/api/claims/search?claimNumber=1' OR '1'='1"
# Expected: 400 Bad Request (validation failure) or empty result
```

---

## 2. Cross-Site Scripting (XSS) Protection

### Status: ⚠️ NEEDS REVIEW

### Verification Steps

- [ ] Review all API responses for user-generated content
- [ ] Implement Content Security Policy (CSP) headers
- [ ] Verify React auto-escaping is working correctly
- [ ] Sanitize user inputs before storage

### Implementation Required

**Backend (ASP.NET Core)**:

Add CSP headers to `Program.cs`:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' https://localhost:5001");

    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

    await next();
});
```

**Frontend (React)**:

React automatically escapes content by default, which protects against XSS:

```tsx
// SAFE - React automatically escapes by default
<div>{userInput}</div>
<input value={userInput} />

// SAFE - Attribute values are also escaped
<div className={userClassName}>{content}</div>
```

**Security Best Practices:**

- Never render unescaped HTML from user input
- Always use React's built-in escaping mechanisms
- Validate and sanitize all user inputs on the backend
- Use Content Security Policy headers to prevent inline script execution
- Avoid using any APIs that evaluate strings as code

### Action Items

1. Add security headers middleware to Program.cs
2. Review all user input fields (claim notes, protocol numbers, etc.)
3. Implement input validation and sanitization in backend validators
4. Test with XSS test payloads to verify protection
5. Ensure all React components use safe rendering patterns

---

## 3. Cross-Site Request Forgery (CSRF) Protection

### Status: ✅ IMPLEMENTED (JWT Bearer Token)

### Verification Steps

- [x] API uses JWT Bearer authentication (stateless)
- [x] SameSite cookie attribute configured
- [ ] Anti-forgery tokens for form submissions (if using cookies)

### Implementation Details

JWT-based authentication provides CSRF protection because:
- Tokens stored in localStorage/sessionStorage (not cookies)
- Custom header required: `Authorization: Bearer <token>`
- Cross-origin requests cannot access localStorage

**If using cookies** (e.g., for session management), add anti-forgery tokens:

```csharp
// In Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-Token";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
```

### Current Configuration

CORS already configured with credentials support:

```csharp
policy.AllowCredentials()
    .WithOrigins("http://localhost:5173", "https://localhost:5173")
```

### Testing

```bash
# CSRF attack should fail (no Authorization header accessible from other origins)
curl -X POST https://localhost:5001/api/claims/authorize-payment \
  -H "Origin: https://attacker.com" \
  --data '{"claimNumber": 123}'
# Expected: 401 Unauthorized
```

---

## 4. Sensitive Data Exposure

### Status: ⚠️ NEEDS REVIEW

### Verification Steps

- [ ] Remove stack traces from production error responses
- [ ] Mask connection strings in logs
- [ ] Exclude sensitive fields from API responses
- [ ] Implement proper logging filters

### Implementation Required

**Production Error Handler** (update `GlobalExceptionHandlerMiddleware.cs`):

```csharp
if (env.IsProduction())
{
    // Generic error message, no stack trace
    await context.Response.WriteAsJsonAsync(new
    {
        error = "Ocorreu um erro interno no servidor.",
        errorCode = errorId,
        timestamp = DateTime.UtcNow
    });
}
else
{
    // Development: full details
    await context.Response.WriteAsJsonAsync(new
    {
        error = exception.Message,
        stackTrace = exception.StackTrace,
        errorCode = errorId
    });
}
```

**Logging Filter** (update `Program.cs` Serilog configuration):

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Destructure.ByTransforming<string>(s =>
        s.Contains("password", StringComparison.OrdinalIgnoreCase) ? "***REDACTED***" : s)
    .WriteTo.Console()
    .WriteTo.File("logs/claims-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

**Exclude Sensitive Fields**:

Ensure DTOs don't include internal fields:

```csharp
// ✅ GOOD - DTO without sensitive data
public class ClaimResponseDto
{
    public int ClaimNumber { get; set; }
    public decimal Amount { get; set; }
    // No internal IDs, audit fields, passwords, etc.
}
```

### Action Items

1. Update `GlobalExceptionHandlerMiddleware.cs` with environment-specific error messages
2. Configure Serilog destructuring for passwords/secrets
3. Review all DTOs for sensitive field exposure
4. Test error responses in production mode

---

## 5. Insecure Dependencies

### Status: ⏳ ONGOING MONITORING

### Verification Steps

- [ ] Run `dotnet list package --vulnerable` regularly
- [ ] Update packages with known vulnerabilities
- [ ] Review NuGet package sources
- [ ] Monitor GitHub security advisories

### Implementation

**Check for vulnerable packages**:

```bash
cd backend/src/CaixaSeguradora.Api
dotnet list package --vulnerable --include-transitive
```

**Update vulnerable packages**:

```bash
# List outdated packages
dotnet list package --outdated

# Update specific package
dotnet add package <PackageName> --version <LatestVersion>
```

**Current Dependencies** (as of 2025-10-27):

| Package                       | Version | Vulnerability Status |
|-------------------------------|---------|----------------------|
| Microsoft.EntityFrameworkCore | 9.0.x   | ✅ None              |
| Serilog                       | 4.0.0   | ✅ None              |
| AspNetCoreRateLimit           | 5.0.0   | ✅ None              |
| FluentValidation              | 11.9.0  | ✅ None              |
| AutoMapper                    | 12.0.1  | ✅ None              |
| SoapCore                      | 1.1.0.2 | ⚠️ Check regularly   |

### Action Items

1. Set up automated dependency scanning (GitHub Dependabot or Snyk)
2. Schedule monthly dependency reviews
3. Document update process for critical vulnerabilities

---

## 6. HTTPS Enforcement

### Status: ⚠️ NEEDS PRODUCTION HARDENING

### Verification Steps

- [x] HTTPS redirection enabled
- [ ] HSTS headers configured
- [ ] Production certificate validation
- [ ] Disable HTTP endpoint in production

### Implementation Required

**Add HSTS Headers** (update `Program.cs`):

```csharp
if (!app.Environment.IsDevelopment())
{
    // HSTS - force HTTPS for 1 year
    app.UseHsts();

    // Additional security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Strict-Transport-Security",
            "max-age=31536000; includeSubDomains; preload");
        await next();
    });
}

app.UseHttpsRedirection();
```

**Production Certificate**:

Configure SSL certificate in appsettings.Production.json:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/caixaseguradora.pfx",
          "Password": "env:CERT_PASSWORD"
        }
      }
    }
  }
}
```

### Testing

```bash
# Verify HTTPS redirect
curl -I http://localhost:5000
# Expected: 307 Temporary Redirect → https://localhost:5001

# Check HSTS header
curl -I https://localhost:5001
# Expected: Strict-Transport-Security: max-age=31536000
```

---

## 7. JWT Security

### Status: ⚠️ NEEDS PRODUCTION HARDENING

### Verification Steps

- [x] JWT signature validation enabled
- [ ] Use strong secret key (min 256-bit)
- [ ] Short token expiration (< 30 minutes)
- [ ] Refresh token implementation
- [ ] Secure secret storage (Azure Key Vault)

### Current Configuration

**appsettings.json** (Development):

```json
{
  "JWT": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMinimum32Chars!",
    "Issuer": "CaixaSeguradoraAPI",
    "Audience": "CaixaSeguradoraClients",
    "ExpirationMinutes": 480
  }
}
```

### Issues & Recommendations

1. **❌ Secret Key in appsettings.json**:
   - Move to Azure Key Vault or User Secrets
   - Use at least 512-bit key (64 characters)

2. **❌ 8-hour expiration too long**:
   - Reduce to 15-30 minutes
   - Implement refresh tokens

3. **✅ Proper validation**:
   - Issuer, Audience, Lifetime all validated
   - `ClockSkew = TimeSpan.Zero` prevents token reuse

### Production Configuration

**Use Azure Key Vault**:

```csharp
// Program.cs
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());

var jwtSecretKey = builder.Configuration["JWT-SecretKey"]; // From Key Vault
```

**appsettings.Production.json**:

```json
{
  "JWT": {
    "SecretKey": "{{REMOVED - Use Key Vault}}",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "KeyVault": {
    "Url": "https://caixaseguradora-kv.vault.azure.net/"
  }
}
```

### Action Items

1. Generate strong JWT secret (512-bit): `openssl rand -base64 64`
2. Store secret in Azure Key Vault
3. Reduce token expiration to 15 minutes
4. Implement refresh token endpoint
5. Test token expiration and renewal

---

## 8. CORS Configuration

### Status: ⚠️ NEEDS PRODUCTION HARDENING

### Verification Steps

- [x] CORS configured
- [ ] Production origins whitelisted
- [ ] Wildcard origins removed
- [ ] Credential support properly configured

### Current Configuration

```csharp
policy.WithOrigins(
    "http://localhost:5173",  // Development
    "https://localhost:5173",
    "http://localhost:3000",
    "https://localhost:3000",
    builder.Configuration["FrontendUrl"] ?? "http://localhost:5173"
)
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials();
```

### Production Configuration

**appsettings.Production.json**:

```json
{
  "FrontendUrl": "https://siwea.caixaseguradora.com.br"
}
```

**Update Program.cs** for production:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsProduction())
        {
            // Production: strict origin whitelist
            policy.WithOrigins(
                builder.Configuration["FrontendUrl"] ??
                    throw new InvalidOperationException("FrontendUrl not configured")
            );
        }
        else
        {
            // Development: allow localhost
            policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:3000"
            );
        }

        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Authorization", "Content-Disposition");
    });
});
```

### Testing

```bash
# Test CORS preflight
curl -X OPTIONS https://localhost:5001/api/claims/search \
  -H "Origin: https://malicious-site.com" \
  -H "Access-Control-Request-Method: GET"
# Expected: No Access-Control-Allow-Origin header (request blocked)
```

---

## 9. Rate Limiting

### Status: ✅ IMPLEMENTED (T139)

### Verification Steps

- [x] AspNetCoreRateLimit package installed
- [x] General rate limits configured (100/min)
- [x] Endpoint-specific limits configured
- [x] Localhost whitelisted for development
- [x] 429 status code returned on limit exceeded

### Current Configuration

**appsettings.json**:

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      { "Endpoint": "*", "Period": "1m", "Limit": 100 }
    ],
    "EndpointRules": [
      { "Endpoint": "GET:/api/claims/search", "Period": "1m", "Limit": 20 },
      { "Endpoint": "POST:/api/claims/authorize-payment", "Period": "1m", "Limit": 10 },
      { "Endpoint": "GET:/api/dashboard/*", "Period": "1m", "Limit": 60 }
    ],
    "IpWhitelist": [ "127.0.0.1", "::1" ]
  }
}
```

### Testing

```bash
# Test rate limiting
for i in {1..25}; do
  curl -s -o /dev/null -w "%{http_code}\n" \
    https://localhost:5001/api/claims/search?claimNumber=123
done
# Expected: First 20 return 200/404, remaining return 429
```

### Production Considerations

- Consider using distributed cache (Redis) instead of in-memory for multi-instance deployments
- Add monitoring for rate limit violations
- Implement IP reputation system for repeat offenders

---

## 10. Additional Security Hardening

### API Security

- [ ] Implement API versioning (`/api/v1/claims`)
- [ ] Add request size limits (prevent DOS via large payloads)
- [ ] Implement query complexity limits (prevent expensive queries)
- [ ] Add API key authentication for service-to-service calls

### Database Security

- [ ] Use least-privilege database user (no DDL permissions)
- [ ] Enable database encryption at rest
- [ ] Implement column-level encryption for sensitive fields
- [ ] Regular database backups with encryption

### Logging & Monitoring

- [ ] Implement security event logging (failed auth attempts, suspicious activity)
- [ ] Set up alerting for security events
- [ ] Use structured logging with correlation IDs
- [ ] Centralized log management (Azure Application Insights)

### Container Security (Docker)

- [ ] Use official Microsoft base images
- [ ] Run container as non-root user
- [ ] Scan images for vulnerabilities
- [ ] Implement secrets management

### Azure Deployment Security

- [ ] Enable Azure App Service managed identity
- [ ] Configure Azure Front Door with WAF
- [ ] Use Azure Key Vault for all secrets
- [ ] Enable Azure Security Center recommendations
- [ ] Implement network security groups (NSGs)

---

## Security Testing Tools

### Recommended Tools

1. **OWASP ZAP** (Dynamic Application Security Testing):
   ```bash
   docker run -t owasp/zap2docker-stable zap-baseline.py \
     -t https://localhost:5001 -r zap-report.html
   ```

2. **dotnet-retire** (Check for vulnerable NuGet packages):
   ```bash
   dotnet tool install --global dotnet-retire
   dotnet retire --loglevel debug
   ```

3. **TruffleHog** (Scan for secrets in git history):
   ```bash
   docker run --rm -v "$(pwd):/proj" trufflesecurity/trufflehog \
     filesystem /proj --json
   ```

4. **SonarQube** (Static code analysis):
   ```bash
   dotnet tool install --global dotnet-sonarscanner
   dotnet sonarscanner begin /k:"CaixaSeguradora"
   dotnet build
   dotnet sonarscanner end
   ```

---

## Security Incident Response Plan

### In Case of Security Breach

1. **Immediate Actions**:
   - Isolate affected systems
   - Revoke all JWT tokens (change secret key)
   - Review access logs for unauthorized activity
   - Notify security team and stakeholders

2. **Investigation**:
   - Analyze logs to determine scope of breach
   - Identify compromised accounts/data
   - Document timeline of events

3. **Remediation**:
   - Patch vulnerability
   - Reset all user credentials
   - Deploy security updates
   - Monitor for continued suspicious activity

4. **Post-Incident**:
   - Conduct security review
   - Update security policies
   - Implement additional safeguards
   - Document lessons learned

---

## Pre-Production Security Checklist

### Before deploying to production, verify:

- [ ] All secrets moved to Azure Key Vault
- [ ] HTTPS enforced with HSTS headers
- [ ] JWT expiration reduced to 15-30 minutes
- [ ] CORS origins limited to production URLs
- [ ] Error messages don't expose stack traces
- [ ] Database user has least-privilege permissions
- [ ] Rate limiting configured appropriately
- [ ] Dependency scan shows no vulnerabilities
- [ ] Security headers (CSP, X-Frame-Options, etc.) configured
- [ ] Logging filters redact sensitive data
- [ ] OWASP ZAP scan completed with no critical issues
- [ ] Security incident response plan documented
- [ ] Security training completed for development team

---

## Maintenance Schedule

### Weekly
- Review access logs for suspicious activity
- Monitor rate limit violations

### Monthly
- Run `dotnet list package --vulnerable`
- Update dependencies with security patches
- Review security logs and incidents

### Quarterly
- Full OWASP ZAP security scan
- Review and update security policies
- Penetration testing (if budget allows)
- Security training refresher for team

### Annually
- Comprehensive security audit by external firm
- Update incident response plan
- Review and renew SSL certificates
- Disaster recovery drill

---

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Azure Security Documentation](https://learn.microsoft.com/en-us/azure/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [CORS Best Practices](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
- [React Security Best Practices](https://react.dev/learn/security)

---

**Document Version**: 1.0
**Last Review**: 2025-10-27
**Next Review**: 2025-11-27
**Owner**: Security Team / DevOps Lead
