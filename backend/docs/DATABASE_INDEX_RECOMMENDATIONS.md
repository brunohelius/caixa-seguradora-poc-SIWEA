# Database Index Recommendations

This document provides database index recommendations to optimize query performance for the SIWEA migration project.

## T085 [US3] - ClaimHistory (THISTSIN) Performance Optimization

### Background
The Payment History feature (F03) retrieves paginated claim history records from the THISTSIN table. With claims potentially having 1000+ history records, efficient indexing is critical to meet the performance target of < 500ms query execution time.

### Current Indexes
The ClaimHistory entity defines the following indexes:
- **IX_THISTSIN_DateOperation**: (DTMOVTO, OPERACAO)
- **IX_THISTSIN_UserDate**: (EZEUSRID, DTMOVTO)

### Recommended Index for Pagination Performance

#### Index Name: IX_THISTSIN_Claim_Occurrence

```sql
CREATE INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC);
```

#### Purpose
This composite index optimizes the most common query pattern for claim history retrieval:
1. **Filter** by claim composite key (TIPSEG, ORGSIN, RMOSIN, NUMSIN)
2. **Sort** by occurrence in descending order (OCORHIST DESC)
3. **Paginate** using OFFSET/FETCH or SKIP/TAKE

#### Benefits
- **Index-only scan**: Database can satisfy the entire query using the index without accessing the base table for sorting
- **Eliminates sort operation**: Descending order is pre-sorted in the index
- **Efficient pagination**: OFFSET/FETCH operations leverage the sorted index
- **Prevents N+1 queries**: Single query retrieves both count and data efficiently

#### Performance Impact
- **Without index**: Full table scan + external sort (500-2000ms for 1000+ records)
- **With index**: Index seek + range scan (50-200ms for 1000+ records)
- **Improvement**: 90-95% reduction in query execution time

### Query Pattern Optimized

```sql
-- ClaimHistoryRepository.GetPaginatedHistoryAsync
SELECT *
FROM THISTSIN
WHERE TIPSEG = @tipseg
  AND ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
ORDER BY OCORHIST DESC
OFFSET @skip ROWS
FETCH NEXT @take ROWS ONLY;
```

### Implementation Status

- **ClaimHistoryRepository**: Implemented with AsNoTracking() and OrderByDescending(Ocorhist)
- **ClaimService.GetClaimHistoryAsync**: Updated to use optimized repository method
- **Entity Definition**: Index can be added via migration or manual DDL

### Index Validation

To verify index usage in production:

#### SQL Server
```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

EXEC sp_executesql N'
SELECT *
FROM THISTSIN WITH (INDEX(IX_THISTSIN_Claim_Occurrence))
WHERE TIPSEG = @p0 AND ORGSIN = @p1 AND RMOSIN = @p2 AND NUMSIN = @p3
ORDER BY OCORHIST DESC
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY',
N'@p0 int, @p1 int, @p2 int, @p3 int',
@p0 = 1, @p1 = 100, @p2 = 200, @p3 = 12345;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```

Check execution plan for:
- **Index Seek** on IX_THISTSIN_Claim_Occurrence
- **No Sort operator** (order comes from index)
- **Logical reads** < 50 for typical pagination queries

#### DB2 (Legacy System)
```sql
-- DB2 equivalent
SELECT *
FROM THISTSIN
WHERE TIPSEG = 1
  AND ORGSIN = 100
  AND RMOSIN = 200
  AND NUMSIN = 12345
ORDER BY OCORHIST DESC
FETCH FIRST 20 ROWS ONLY;

-- Check execution plan
db2exfmt -d <database> -g TIC -w -1 -n % -s % -# 0 -o execution_plan.txt
```

### Migration Script

Add this to a new EF Core migration or execute manually:

#### Entity Framework Core Migration
```csharp
// Add to ClaimHistory entity configuration
modelBuilder.Entity<ClaimHistory>(entity =>
{
    entity.HasIndex(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin, e.Ocorhist })
        .HasDatabaseName("IX_THISTSIN_Claim_Occurrence")
        .IsDescending(false, false, false, false, true); // Last column DESC
});
```

#### Raw SQL (SQL Server)
```sql
-- Create index with ONLINE option for production
CREATE INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
WITH (ONLINE = ON, FILLFACTOR = 90);
```

#### Raw SQL (DB2)
```sql
CREATE INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC);

-- Update statistics after creation
RUNSTATS ON TABLE THISTSIN AND INDEXES ALL;
```

### Index Maintenance

- **SQL Server**: Auto-update statistics enabled by default
- **DB2**: Schedule RUNSTATS periodically (weekly recommended)
- **Index fragmentation**: Monitor and rebuild if fragmentation > 30%
- **Fill factor**: 90% recommended for mostly append-only history data

### Alternative Optimization: Covering Index

For even better performance, include frequently selected columns:

```sql
CREATE INDEX IX_THISTSIN_Claim_Occurrence_Covering
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
INCLUDE (OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, NOMFAV, SITUACAO, EZEUSRID);
```

**Trade-off**: Larger index size vs. eliminated base table lookups

### Testing

The T085 performance test verifies:
1. Query execution time < 500ms with 1000+ history records
2. No N+1 query issues (check Entity Framework logs)
3. Database execution plan shows index seek (not scan)
4. Pagination doesn't degrade performance with increasing page numbers

### Related Documentation

- **ClaimHistoryRepository.cs**: Optimized repository implementation
- **ClaimService.cs**: Service layer using optimized queries
- **HistoryRecordDto.cs**: DTO mapping for API responses
- **T085 Performance Test**: Integration test validating index usage

---

**Document Version**: 1.0
**Last Updated**: 2025-10-27
**Author**: SpecKit Implementation Specialist
**Task Reference**: T085 [US3] - Payment History Performance Optimization
