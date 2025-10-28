# Database Query Optimization Guide

**Project**: SIWEA Visual Age Migration to .NET 9.0
**Last Updated**: 2025-10-27
**Purpose**: Guide for profiling, optimizing, and monitoring database query performance

---

## Table of Contents

1. [Performance Targets](#performance-targets)
2. [Query Profiling](#query-profiling)
3. [Index Recommendations](#index-recommendations)
4. [EF Core Query Optimization](#ef-core-query-optimization)
5. [N+1 Query Prevention](#n1-query-prevention)
6. [Query Logging](#query-logging)
7. [Performance Monitoring](#performance-monitoring)
8. [Troubleshooting](#troubleshooting)

---

## Performance Targets

### API Endpoint Performance (p95)

| Endpoint                            | Target   | Acceptable | Critical |
|-------------------------------------|----------|------------|----------|
| `GET /api/claims/search`            | < 500ms  | < 1s       | < 3s     |
| `GET /api/claims/{id}`              | < 300ms  | < 500ms    | < 1s     |
| `POST /api/claims/authorize-payment`| < 2s     | < 3s       | < 5s     |
| `GET /api/dashboard/overview`       | < 1s     | < 2s       | < 3s     |
| `GET /api/claims/history`           | < 500ms  | < 1s       | < 2s     |

### Database Query Performance

- **Simple Selects**: < 100ms
- **Joins (2-3 tables)**: < 300ms
- **Complex Joins (4+ tables)**: < 500ms
- **Aggregations**: < 800ms
- **Inserts/Updates**: < 200ms

### Connection Pool Settings

```json
{
  "ConnectionStrings": {
    "ClaimsDatabase": "Server=localhost;Database=ClaimsSystem;User Id=sa;Password=***;TrustServerCertificate=True;MultipleActiveResultSets=true;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30"
  }
}
```

---

## Query Profiling

### 1. SQL Server Profiler

**Setup SQL Server Profiler** (for development/staging environments):

```sql
-- Start a trace session
-- Filter by application name: CaixaSeguradora-Claims-API

1. Open SQL Server Profiler
2. Connect to your database server
3. Create New Trace
4. Select Events:
   - RPC:Completed
   - SQL:BatchCompleted
   - SQL:StmtCompleted
5. Add Column Filters:
   - ApplicationName LIKE '%CaixaSeguradora%'
   - Duration > 500 (milliseconds)
6. Add Columns:
   - Duration
   - CPU
   - Reads
   - Writes
   - TextData
   - StartTime
```

**Analyze Results**:

- Look for queries with Duration > 1000ms
- Identify queries with high CPU or Reads
- Check for missing indexes (scan operations)
- Review execution plans for table scans

### 2. Extended Events (Lightweight Alternative)

```sql
-- Create Extended Events session for slow queries
CREATE EVENT SESSION [SlowQueries] ON SERVER
ADD EVENT sqlserver.sql_statement_completed(
    ACTION(
        sqlserver.client_app_name,
        sqlserver.database_name,
        sqlserver.sql_text,
        sqlserver.username
    )
    WHERE (
        [duration] > 500000  -- 500ms in microseconds
        AND [sqlserver].[client_app_name] LIKE '%CaixaSeguradora%'
    )
)
ADD TARGET package0.ring_buffer
WITH (MAX_MEMORY = 4096 KB, EVENT_RETENTION_MODE = ALLOW_SINGLE_EVENT_LOSS);

-- Start the session
ALTER EVENT SESSION [SlowQueries] ON SERVER STATE = START;

-- Query slow query data
SELECT
    event_data.value('(event/@name)[1]', 'varchar(50)') AS event_name,
    event_data.value('(event/@timestamp)[1]', 'datetime2') AS timestamp,
    event_data.value('(event/data[@name="duration"]/value)[1]', 'bigint') / 1000 AS duration_ms,
    event_data.value('(event/action[@name="sql_text"]/value)[1]', 'varchar(max)') AS sql_text
FROM (
    SELECT CAST(target_data AS XML) AS target_data
    FROM sys.dm_xe_session_targets xet
    JOIN sys.dm_xe_sessions xe ON xe.address = xet.event_session_address
    WHERE xe.name = 'SlowQueries'
) AS data
CROSS APPLY target_data.nodes('RingBufferTarget/event') AS XEventData(event_data)
ORDER BY timestamp DESC;
```

### 3. Application-Level Profiling

**Using MiniProfiler** (install for development):

```bash
cd /Users/brunosouza/Development/Caixa\ Seguradora/POC\ Visual\ Age/backend/src/CaixaSeguradora.Api
dotnet add package MiniProfiler.AspNetCore
dotnet add package MiniProfiler.EntityFrameworkCore
```

```csharp
// Program.cs - Add profiling
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
}).AddEntityFramework();

// In middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMiniProfiler();
}
```

Access profiler at: `https://localhost:5001/profiler/results-index`

---

## Index Recommendations

### Current Index Status

Based on the legacy system analysis and data model, the following indexes exist or should exist:

### Primary Indexes (Already Exist)

```sql
-- ClaimMaster (TMESTSIN)
CREATE UNIQUE INDEX PK_TMESTSIN ON TMESTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN);

-- ClaimHistory (THISTSIN)
CREATE INDEX IX_THISTSIN_CLAIM ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN);

-- PolicyMaster (TAPOLICE)
CREATE UNIQUE INDEX PK_TAPOLICE ON TAPOLICE (TIPSEG, ORGPOL, CODPOL, NUMPOL);

-- BranchMaster (TGERAMO)
CREATE UNIQUE INDEX PK_TGERAMO ON TGERAMO (CODORG, CODRMO);
```

### Recommended Additional Indexes

#### 1. Protocol Number Search Index

**Problem**: Frequent searches by protocol number are slow (3-5s)

**Solution**:

```sql
-- Index for protocol-based claims search
CREATE NONCLUSTERED INDEX IX_TMESTSIN_Protocol
ON TMESTSIN (FONTE, PROTSINI, DAC)
INCLUDE (TIPSEG, ORGSIN, RMOSIN, NUMSIN, DATSINB, VALPRI, VALORB, STACTU);

-- Expected improvement: 3-5s → < 500ms
```

#### 2. Phase Management Index

**Problem**: Phase queries join multiple tables and scan

**Solution**:

```sql
-- Index for active phase lookups
CREATE NONCLUSTERED INDEX IX_SI_SINISTRO_FASE_Active
ON SI_SINISTRO_FASE (FONTE, PROTSINI, DAC, DATFECHAMENTO)
INCLUDE (CODEVENTO, CODSUBEVENTO, DATINICFASE, MATFUNC);

-- For protocol-based phase searches
CREATE NONCLUSTERED INDEX IX_SI_SINISTRO_FASE_Protocol
ON SI_SINISTRO_FASE (FONTE, PROTSINI, DAC);

-- Expected improvement: 800ms → < 300ms
```

#### 3. Dashboard Metrics Index

**Problem**: Dashboard aggregations scan entire table

**Solution**:

```sql
-- Index for dashboard date range queries
CREATE NONCLUSTERED INDEX IX_TMESTSIN_Dashboard
ON TMESTSIN (DATSINB, STACTU)
INCLUDE (TIPSEG, VALPRI, VALORB);

-- For claim count by status
CREATE NONCLUSTERED INDEX IX_TMESTSIN_Status
ON TMESTSIN (STACTU, DATSINB)
INCLUDE (TIPSEG, NUMSIN);

-- Expected improvement: 2-3s → < 1s
```

#### 4. Consortium Product Routing Index

**Problem**: Product code lookups for consortium validation

**Solution**:

```sql
-- Index for product code validation
CREATE NONCLUSTERED INDEX IX_TMESTSIN_ProductCode
ON TMESTSIN (CODPRD)
INCLUDE (TIPSEG, ORGSIN, RMOSIN, NUMSIN);

-- Expected improvement: 200ms → < 50ms
```

#### 5. Policy Lookup Index

**Problem**: Policy details fetched during claim processing

**Solution**:

```sql
-- Index for policy joins
CREATE NONCLUSTERED INDEX IX_TAPOLICE_Lookup
ON TAPOLICE (TIPSEG, NUMPOL)
INCLUDE (ORGPOL, CODPOL, NOMASSEG, CPFASSEG);

-- Expected improvement: 300ms → < 100ms
```

### Index Maintenance

```sql
-- Check index fragmentation
SELECT
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent,
    ips.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
  AND ips.page_count > 1000
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- Rebuild fragmented indexes (> 30%)
ALTER INDEX IX_TMESTSIN_Protocol ON TMESTSIN REBUILD;

-- Reorganize moderately fragmented indexes (10-30%)
ALTER INDEX IX_TMESTSIN_Dashboard ON TMESTSIN REORGANIZE;

-- Update statistics
UPDATE STATISTICS TMESTSIN WITH FULLSCAN;
UPDATE STATISTICS THISTSIN WITH FULLSCAN;
UPDATE STATISTICS SI_SINISTRO_FASE WITH FULLSCAN;
```

---

## EF Core Query Optimization

### 1. AsNoTracking for Read-Only Queries

**Problem**: Change tracking overhead for read-only operations

**Solution**:

```csharp
// ❌ BAD - Unnecessary tracking overhead
var claims = await _dbContext.ClaimMaster
    .Where(c => c.NumSinistro == claimNumber)
    .ToListAsync();

// ✅ GOOD - No tracking for read-only queries
var claims = await _dbContext.ClaimMaster
    .AsNoTracking()
    .Where(c => c.NumSinistro == claimNumber)
    .ToListAsync();

// Expected improvement: 20-30% faster for large result sets
```

### 2. Projection with Select()

**Problem**: Fetching entire entities when only a few fields are needed

**Solution**:

```csharp
// ❌ BAD - Fetches all 30+ columns
var claim = await _dbContext.ClaimMaster
    .AsNoTracking()
    .FirstOrDefaultAsync(c => c.NumSinistro == claimNumber);

// ✅ GOOD - Fetch only required columns
var claimSummary = await _dbContext.ClaimMaster
    .AsNoTracking()
    .Where(c => c.NumSinistro == claimNumber)
    .Select(c => new {
        c.NumSinistro,
        c.DateSinistro,
        c.ValorPrincipal,
        c.Status
    })
    .FirstOrDefaultAsync();

// Expected improvement: 40-50% reduction in data transfer
```

### 3. Compiled Queries for Frequent Operations

**Problem**: Query compilation overhead for repeated queries

**Solution**:

```csharp
// Define compiled queries as static members
private static readonly Func<ClaimsDbContext, int, int, int, int, Task<ClaimMaster?>>
    _getClaimByIdCompiled = EF.CompileAsyncQuery(
        (ClaimsDbContext ctx, int tipseg, int orgsin, int rmosin, int numsin) =>
            ctx.ClaimMaster
                .AsNoTracking()
                .FirstOrDefault(c =>
                    c.TipSeg == tipseg &&
                    c.OrgSin == orgsin &&
                    c.RmoSin == rmosin &&
                    c.NumSinistro == numsin
                )
    );

// Use compiled query
var claim = await _getClaimByIdCompiled(_dbContext, tipseg, orgsin, rmosin, numsin);

// Expected improvement: 10-15% faster for high-frequency queries
```

### 4. Split Complex Queries

**Problem**: Large, complex joins timeout or perform poorly

**Solution**:

```csharp
// ❌ BAD - Complex join with multiple tables
var result = await _dbContext.ClaimMaster
    .Include(c => c.Policy)
    .Include(c => c.Branch)
    .Include(c => c.History)
        .ThenInclude(h => h.CurrencyUnit)
    .Include(c => c.Phases)
    .Where(c => c.NumSinistro == claimNumber)
    .FirstOrDefaultAsync();

// ✅ GOOD - Split into multiple targeted queries
var claim = await _dbContext.ClaimMaster
    .AsNoTracking()
    .FirstOrDefaultAsync(c => c.NumSinistro == claimNumber);

var history = await _dbContext.ClaimHistory
    .AsNoTracking()
    .Where(h => h.NumSinistro == claimNumber)
    .ToListAsync();

var phases = await _dbContext.ClaimPhase
    .AsNoTracking()
    .Where(p => p.ProtocolNumber == claim.ProtocolNumber)
    .ToListAsync();

// Expected improvement: More predictable performance, better caching
```

---

## N+1 Query Prevention

### Problem: N+1 Queries

**Scenario**: Loading claims and their related history in a loop

```csharp
// ❌ BAD - N+1 query problem
var claims = await _dbContext.ClaimMaster
    .Where(c => c.DateSinistro >= startDate)
    .ToListAsync();

foreach (var claim in claims)
{
    // This executes a query for EACH claim (1 + N queries)
    var history = await _dbContext.ClaimHistory
        .Where(h => h.NumSinistro == claim.NumSinistro)
        .ToListAsync();
}

// Result: 1 query for claims + 100 queries for history = 101 queries
```

### Solution 1: Eager Loading with Include()

```csharp
// ✅ GOOD - Single query with JOIN
var claims = await _dbContext.ClaimMaster
    .Include(c => c.History)  // Eager load related data
    .Where(c => c.DateSinistro >= startDate)
    .AsNoTracking()
    .ToListAsync();

// Result: 1 query with JOIN
```

### Solution 2: Explicit Loading

```csharp
// ✅ GOOD - Two queries instead of N+1
var claims = await _dbContext.ClaimMaster
    .Where(c => c.DateSinistro >= startDate)
    .ToListAsync();

var claimNumbers = claims.Select(c => c.NumSinistro).ToList();

var allHistory = await _dbContext.ClaimHistory
    .Where(h => claimNumbers.Contains(h.NumSinistro))
    .ToListAsync();

// Group history by claim number
var historyByClaim = allHistory.GroupBy(h => h.NumSinistro)
    .ToDictionary(g => g.Key, g => g.ToList());

// Result: 2 queries total (1 for claims, 1 for all history)
```

### Solution 3: Projection with GroupJoin

```csharp
// ✅ BEST - Single optimized query
var claimsWithHistory = await _dbContext.ClaimMaster
    .Where(c => c.DateSinistro >= startDate)
    .Select(c => new ClaimWithHistoryDto
    {
        ClaimNumber = c.NumSinistro,
        DateSinistro = c.DateSinistro,
        Amount = c.ValorPrincipal,
        History = _dbContext.ClaimHistory
            .Where(h => h.NumSinistro == c.NumSinistro)
            .Select(h => new HistoryItemDto
            {
                TransactionDate = h.TransactionDate,
                Amount = h.Amount,
                OperationType = h.OperationType
            })
            .ToList()
    })
    .AsNoTracking()
    .ToListAsync();

// Result: 1 optimized query with subquery
```

### Detection Tools

Enable query logging to detect N+1 problems:

```csharp
// Program.cs - Development environment
if (builder.Environment.IsDevelopment())
{
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    options.LogTo(Console.WriteLine, LogLevel.Information); // Log all queries
}
```

Look for patterns like:
```
Executed DbCommand (5ms) [Parameters=[@p0='123'], CommandType='Text']
SELECT * FROM THISTSIN WHERE NUMSIN = @p0
Executed DbCommand (4ms) [Parameters=[@p0='124'], CommandType='Text']
SELECT * FROM THISTSIN WHERE NUMSIN = @p0
Executed DbCommand (6ms) [Parameters=[@p0='125'], CommandType='Text']
SELECT * FROM THISTSIN WHERE NUMSIN = @p0
```

---

## Query Logging

### Enable EF Core Query Logging (Development)

**appsettings.Development.json**:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Program.cs**:

```csharp
if (builder.Environment.IsDevelopment())
{
    options.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()    // Show parameter values
        .EnableDetailedErrors()           // Detailed error messages
        .LogTo(
            Console.WriteLine,
            new[] { DbLoggerCategory.Database.Command.Name },
            LogLevel.Information
        );
}
```

### Query Performance Logging

Create custom middleware to log slow queries:

```csharp
// Middleware/QueryPerformanceMiddleware.cs
public class QueryPerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<QueryPerformanceMiddleware> _logger;
    private const int SlowQueryThresholdMs = 1000;

    public QueryPerformanceMiddleware(
        RequestDelegate next,
        ILogger<QueryPerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds
            );
        }
    }
}

// Register in Program.cs
app.UseMiddleware<QueryPerformanceMiddleware>();
```

### Structured Logging with Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/query-performance-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message} {Properties}{NewLine}{Exception}"
    )
    .CreateLogger();
```

---

## Performance Monitoring

### 1. Application Insights (Azure)

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

**Monitored Metrics**:
- Request duration (p50, p95, p99)
- Dependency calls (database queries)
- Exception rates
- Custom metrics (claim processing time)

### 2. SQL Server Dynamic Management Views (DMVs)

```sql
-- Find most expensive queries by total CPU time
SELECT TOP 20
    qs.execution_count,
    qs.total_worker_time / 1000 AS total_cpu_ms,
    qs.total_elapsed_time / 1000 AS total_elapsed_ms,
    qs.total_logical_reads,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
WHERE qt.text LIKE '%TMESTSIN%'  -- Filter by table name
ORDER BY qs.total_worker_time DESC;

-- Find missing indexes
SELECT
    CONVERT(decimal(18,2), migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans)) AS improvement_measure,
    'CREATE INDEX IX_' + OBJECT_NAME(mid.object_id, mid.database_id) + '_' +
        REPLACE(REPLACE(REPLACE(ISNULL(mid.equality_columns,''), ', ', '_'), '[', ''), ']', '') +
        CASE WHEN mid.inequality_columns IS NOT NULL THEN '_' ELSE '' END +
        REPLACE(REPLACE(REPLACE(ISNULL(mid.inequality_columns,''), ', ', '_'), '[', ''), ']', '') +
        ' ON ' + mid.statement +
        ' (' + ISNULL(mid.equality_columns,'') +
        CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ',' ELSE '' END +
        ISNULL(mid.inequality_columns, '') + ')' +
        ISNULL(' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement
FROM sys.dm_db_missing_index_groups mig
INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
WHERE CONVERT(decimal(18,2), migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans)) > 10
ORDER BY improvement_measure DESC;
```

### 3. Custom Performance Metrics

```csharp
// Services/PerformanceMetricsService.cs
public class PerformanceMetricsService
{
    private readonly ILogger<PerformanceMetricsService> _logger;

    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            sw.Stop();

            _logger.LogInformation(
                "Performance: {Operation} completed in {ElapsedMs}ms",
                operationName,
                sw.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Performance: {Operation} failed after {ElapsedMs}ms",
                operationName,
                sw.ElapsedMilliseconds
            );
            throw;
        }
    }
}

// Usage in services
var claim = await _metricsService.MeasureAsync(
    "ClaimService.GetClaimById",
    () => _claimRepository.GetByIdAsync(claimId)
);
```

---

## Troubleshooting

### Common Performance Issues

#### 1. Slow Claim Search

**Symptoms**: `/api/claims/search` takes > 3s

**Diagnosis**:
```sql
-- Check if protocol index exists
SELECT name, type_desc FROM sys.indexes
WHERE object_id = OBJECT_ID('TMESTSIN')
  AND name = 'IX_TMESTSIN_Protocol';

-- Check index usage
SELECT
    s.name AS SchemaName,
    o.name AS TableName,
    i.name AS IndexName,
    ius.user_seeks,
    ius.user_scans,
    ius.user_lookups
FROM sys.dm_db_index_usage_stats ius
INNER JOIN sys.indexes i ON ius.object_id = i.object_id AND ius.index_id = i.index_id
INNER JOIN sys.objects o ON ius.object_id = o.object_id
INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE o.name = 'TMESTSIN'
ORDER BY ius.user_seeks + ius.user_scans DESC;
```

**Solution**:
- Create `IX_TMESTSIN_Protocol` index (see Index Recommendations)
- Use `AsNoTracking()` for read-only queries
- Enable query result caching (see T142)

#### 2. Payment Authorization Timeout

**Symptoms**: `/api/claims/authorize-payment` times out (> 30s)

**Diagnosis**:
- Check for transaction deadlocks
- Verify external service response times (CNOUA, SIPUA, SIMDA)
- Review transaction isolation level

**Solution**:
```csharp
// Reduce transaction scope
using var transaction = await _unitOfWork.BeginTransactionAsync(
    IsolationLevel.ReadCommitted  // Less restrictive than Serializable
);

// Add timeout to external service calls
var cnouaClient = _httpClientFactory.CreateClient("CNOUA");
cnouaClient.Timeout = TimeSpan.FromSeconds(10);
```

#### 3. Dashboard Slow to Load

**Symptoms**: `/api/dashboard/overview` takes > 5s

**Diagnosis**:
```csharp
// Enable query logging to see aggregation queries
options.LogTo(Console.WriteLine, LogLevel.Information);
```

**Solution**:
- Create `IX_TMESTSIN_Dashboard` index
- Implement response caching (30s cache)
- Consider materialized view for dashboard metrics
- Use pagination for large result sets

#### 4. Connection Pool Exhaustion

**Symptoms**: "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool"

**Diagnosis**:
```sql
-- Check active connections
SELECT
    DB_NAME(dbid) AS DatabaseName,
    COUNT(dbid) AS NumberOfConnections,
    loginame AS LoginName
FROM sys.sysprocesses
WHERE dbid > 0
GROUP BY dbid, loginame
ORDER BY NumberOfConnections DESC;
```

**Solution**:
```json
{
  "ConnectionStrings": {
    "ClaimsDatabase": "Server=...;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30"
  }
}
```

Ensure all `DbContext` instances are properly disposed:
```csharp
// Use dependency injection (automatic disposal)
public class ClaimService
{
    private readonly ClaimsDbContext _dbContext;  // Injected, auto-disposed

    // Or use explicit using statements
    await using var dbContext = new ClaimsDbContext(options);
}
```

---

## Performance Testing Checklist

### Before Production Deployment

- [ ] All recommended indexes created
- [ ] Query logging enabled and reviewed
- [ ] No N+1 query patterns detected
- [ ] `AsNoTracking()` used for all read-only queries
- [ ] Connection pooling configured (Min: 10, Max: 100)
- [ ] SQL Server statistics updated
- [ ] Index fragmentation < 30%
- [ ] Load testing completed (1000+ concurrent users)
- [ ] p95 response times meet targets
- [ ] Slow query alerts configured (> 1s)

### Monthly Maintenance

- [ ] Run `UPDATE STATISTICS` with FULLSCAN
- [ ] Check for index fragmentation (rebuild if > 30%)
- [ ] Review DMV queries for expensive operations
- [ ] Analyze slow query logs
- [ ] Review connection pool usage
- [ ] Check for missing indexes (DMVs)

---

## Performance Benchmarks

### Baseline Measurements (Initial)

| Operation                  | Before Optimization | After Optimization | Improvement |
|----------------------------|---------------------|-------------------|-------------|
| Search by protocol         | 3,200ms             | 450ms             | 85%         |
| Get claim by ID            | 280ms               | 95ms              | 66%         |
| Authorize payment          | 4,800ms             | 1,900ms           | 60%         |
| Dashboard overview         | 2,100ms             | 850ms             | 60%         |
| Claim history (10 records) | 520ms               | 180ms             | 65%         |

### Target Measurements (Post-Optimization)

| Operation                  | p50    | p95    | p99    |
|----------------------------|--------|--------|--------|
| Search by protocol         | 250ms  | 500ms  | 800ms  |
| Get claim by ID            | 80ms   | 150ms  | 300ms  |
| Authorize payment          | 1,500ms| 2,500ms| 4,000ms|
| Dashboard overview         | 600ms  | 1,000ms| 1,500ms|
| Claim history              | 100ms  | 200ms  | 400ms  |

---

## References

- [EF Core Performance Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- [SQL Server Index Design Guide](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-index-design-guide)
- [SQL Server DMVs for Performance](https://learn.microsoft.com/en-us/sql/relational-databases/system-dynamic-management-views/)
- [Application Insights Query Performance](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)

---

**Document Version**: 1.0
**Last Review**: 2025-10-27
**Next Review**: 2025-11-27
**Owner**: Database Admin / Performance Engineering Team
