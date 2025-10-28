# Performance Notes - Claims History Query

## T085: History Query Performance Verification

### Overview
This document provides recommendations for optimizing the history query performance when dealing with large datasets (1000+ records).

### Current Implementation
**Location**: `backend/src/CaixaSeguradora.Core/Services/ClaimService.cs` → `GetClaimHistoryAsync`

**Query Pattern**:
```csharp
var history = await _unitOfWork.ClaimHistories
    .Where(h => h.Tipseg == tipseg && h.Orgsin == orgsin && h.Rmosin == rmosin && h.Numsin == numsin)
    .OrderByDescending(h => h.Ocorhist)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Performance Requirements
- **Target**: Query execution time < 500ms for 1000+ records
- **Pagination**: 20 records per page (default), max 100 per page
- **Ordering**: By `Ocorhist` DESC (most recent first)

### Recommended Database Indexes

#### Primary Index (Critical)
```sql
CREATE NONCLUSTERED INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN(TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
INCLUDE (OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, NOMFAV, TIPCRR,
         VALPRIBT, CRRMONBT, VALTOTBT, SITCONTB, SITUACAO, EZEUSRID);
```

**Benefits**:
- **Covering Index**: All columns needed for query are included → no table lookups
- **Composite Key**: Filters on claim composite key + ordering column
- **DESC Order**: Matches query ordering, eliminates sort operation
- **Expected improvement**: 80-90% reduction in query time

#### Verification Query
```sql
-- Test index effectiveness
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

SELECT TOP 20
    TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST,
    OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, NOMFAV,
    TIPCRR, VALPRIBT, CRRMONBT, VALTOTBT, SITCONTB, SITUACAO, EZEUSRID
FROM THISTSIN
WHERE TIPSEG = 1 AND ORGSIN = 10 AND RMOSIN = 20 AND NUMSIN = 789012
ORDER BY OCORHIST DESC;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```

**Look for**:
- Logical reads < 10
- Execution time < 50ms
- Index seek (not index scan)

### EF Core Query Optimization

#### 1. Compiled Queries
For frequently executed queries, use compiled queries:

```csharp
private static readonly Func<ClaimsDbContext, int, int, int, int, int, int, IAsyncEnumerable<ClaimHistory>>
    CompiledHistoryQuery = EF.CompileAsyncQuery(
        (ClaimsDbContext context, int tipseg, int orgsin, int rmosin, int numsin, int skip, int take) =>
            context.ClaimHistories
                .Where(h => h.Tipseg == tipseg && h.Orgsin == orgsin && h.Rmosin == rmosin && h.Numsin == numsin)
                .OrderByDescending(h => h.Ocorhist)
                .Skip(skip)
                .Take(take)
    );
```

**Benefits**: 10-20% improvement on repeated queries (cached execution plan)

#### 2. AsNoTracking for Read-Only Queries
```csharp
.AsNoTracking()  // Disable change tracking for read-only operations
```

**Benefits**: Reduces memory allocation and improves query speed by 15-25%

#### 3. Projection to DTOs
Instead of loading full entities:

```csharp
.Select(h => new HistoryRecordDto
{
    Tipseg = h.Tipseg,
    // ... map only needed properties
})
```

**Benefits**: Reduces data transfer and memory usage

### Monitoring and Diagnostics

#### Enable Query Logging (Development)
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

#### Application Insights Query Performance
Track query metrics:
```csharp
using var operation = _telemetryClient.StartOperation<DependencyTelemetry>("ClaimHistory Query");
operation.Telemetry.Data = $"tipseg={tipseg}, page={page}";
// ... execute query
operation.Telemetry.Success = true;
```

### Load Testing Recommendations

#### Test Scenarios
1. **Small dataset**: 10 records, verify < 100ms
2. **Medium dataset**: 100 records, verify < 200ms
3. **Large dataset**: 1000+ records, verify < 500ms
4. **Pagination stress**: Jump to page 50 of 1000 records
5. **Concurrent requests**: 50 simultaneous history queries

#### Sample k6 Load Test
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p95<500'],
  },
};

export default function () {
  let response = http.get(
    'https://localhost:5001/api/claims/1/10/20/789012/history?page=1&pageSize=20'
  );

  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

### Known Limitations

1. **N+1 Query Prevention**: Ensure no navigation properties are loaded unintentionally
2. **Pagination Depth**: Very deep pagination (page 100+) may be slower - consider using cursor-based pagination for infinite scroll
3. **Database Connection Pool**: Ensure sufficient connections configured (default 100)

### Performance Benchmarks (Expected)

| Scenario | Without Index | With Index | With Index + Compiled Query |
|----------|--------------|------------|----------------------------|
| 10 records | 150ms | 45ms | 35ms |
| 100 records | 450ms | 95ms | 75ms |
| 1000 records | 1200ms | 280ms | 220ms |
| 5000 records | 3500ms | 650ms | 480ms |

### Implementation Checklist

- [X] Review current query implementation
- [ ] Create database index (IX_THISTSIN_Claim_Occurrence)
- [ ] Verify index usage with execution plan
- [ ] Implement compiled query optimization
- [ ] Add query performance logging
- [ ] Run load tests with k6 or NBomber
- [ ] Monitor production metrics
- [ ] Document baseline vs optimized performance

### Next Steps

1. **DBA**: Request index creation in non-production environments first
2. **Testing**: Run performance tests before and after index
3. **Monitoring**: Set up Application Insights alerts for slow queries (>500ms)
4. **Review**: Schedule quarterly review of query performance metrics

---

**Last Updated**: 2025-10-23
**Owner**: Backend Team
**Status**: Index recommendation documented, awaiting implementation
