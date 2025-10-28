using FluentValidation;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Performance Rules (BR-096 to BR-099)
/// Enforces business rules for system performance targets
/// Note: These are primarily monitoring/testing rules, not runtime validation
/// </summary>
public class PerformanceRules : AbstractValidator<PerformanceMetricsContext>
{
    public PerformanceRules()
    {
        // BR-096: Claim search < 3 seconds
        RuleFor(x => x.SearchDuration)
            .LessThan(TimeSpan.FromSeconds(3))
            .When(x => x.SearchDuration.HasValue)
            .WithMessage("Busca de sinistro deve ser concluída em menos de 3 segundos")
            .WithErrorCode("BR-096");

        // BR-097: Payment authorization cycle < 90 seconds
        RuleFor(x => x.AuthorizationDuration)
            .LessThan(TimeSpan.FromSeconds(90))
            .When(x => x.AuthorizationDuration.HasValue)
            .WithMessage("Ciclo de autorização de pagamento deve ser concluído em menos de 90 segundos")
            .WithErrorCode("BR-097");

        // BR-098: History query < 500ms for 1000+ records
        RuleFor(x => x.HistoryQueryDuration)
            .LessThan(TimeSpan.FromMilliseconds(500))
            .When(x => x.HistoryQueryDuration.HasValue && x.HistoryRecordCount >= 1000)
            .WithMessage("Consulta de histórico deve ser concluída em menos de 500ms para 1000+ registros")
            .WithErrorCode("BR-098");

        // BR-099: Pagination: 20 records/page default, max 100
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue)
            .WithMessage("Tamanho da página deve estar entre 1 e 100 registros")
            .WithErrorCode("BR-099");

        // Default page size should be 20
        RuleFor(x => x.PageSize)
            .Equal(20)
            .When(x => x.PageSize.HasValue && x.IsDefaultPageSize)
            .WithMessage("Tamanho padrão da página deve ser 20 registros")
            .WithErrorCode("BR-099");
    }
}

/// <summary>
/// Context for performance metrics validation
/// Contains performance measurements for validation
/// </summary>
public class PerformanceMetricsContext
{
    /// <summary>
    /// Duration of claim search operation
    /// </summary>
    public TimeSpan? SearchDuration { get; set; }

    /// <summary>
    /// Duration of payment authorization cycle
    /// </summary>
    public TimeSpan? AuthorizationDuration { get; set; }

    /// <summary>
    /// Duration of history query operation
    /// </summary>
    public TimeSpan? HistoryQueryDuration { get; set; }

    /// <summary>
    /// Number of history records returned
    /// </summary>
    public int HistoryRecordCount { get; set; }

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Whether this is using the default page size
    /// </summary>
    public bool IsDefaultPageSize { get; set; }

    /// <summary>
    /// Operation name for logging/metrics
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// Timestamp when operation started
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Timestamp when operation completed
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Performance target constants (BR-096 to BR-099)
/// </summary>
public static class PerformanceTargets
{
    /// <summary>
    /// BR-096: Maximum claim search duration
    /// </summary>
    public static readonly TimeSpan MaxSearchDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// BR-097: Maximum payment authorization cycle duration
    /// </summary>
    public static readonly TimeSpan MaxAuthorizationDuration = TimeSpan.FromSeconds(90);

    /// <summary>
    /// BR-098: Maximum history query duration for 1000+ records
    /// </summary>
    public static readonly TimeSpan MaxHistoryQueryDuration = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// BR-099: Default page size
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// BR-099: Maximum page size
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// BR-099: Minimum page size
    /// </summary>
    public const int MinPageSize = 1;
}

/// <summary>
/// Performance monitoring utilities
/// </summary>
public static class PerformanceMonitoring
{
    /// <summary>
    /// Validates if search operation meets performance target
    /// </summary>
    public static bool IsSearchPerformanceAcceptable(TimeSpan duration)
    {
        return duration < PerformanceTargets.MaxSearchDuration;
    }

    /// <summary>
    /// Validates if authorization operation meets performance target
    /// </summary>
    public static bool IsAuthorizationPerformanceAcceptable(TimeSpan duration)
    {
        return duration < PerformanceTargets.MaxAuthorizationDuration;
    }

    /// <summary>
    /// Validates if history query meets performance target
    /// </summary>
    public static bool IsHistoryQueryPerformanceAcceptable(TimeSpan duration, int recordCount)
    {
        if (recordCount < 1000)
            return true; // Performance target only applies to 1000+ records

        return duration < PerformanceTargets.MaxHistoryQueryDuration;
    }

    /// <summary>
    /// Validates page size is within acceptable range
    /// </summary>
    public static bool IsPageSizeValid(int pageSize)
    {
        return pageSize >= PerformanceTargets.MinPageSize &&
               pageSize <= PerformanceTargets.MaxPageSize;
    }

    /// <summary>
    /// Gets normalized page size (default if not specified, clamped to valid range)
    /// </summary>
    public static int GetNormalizedPageSize(int? pageSize)
    {
        if (!pageSize.HasValue)
            return PerformanceTargets.DefaultPageSize;

        if (pageSize.Value < PerformanceTargets.MinPageSize)
            return PerformanceTargets.MinPageSize;

        if (pageSize.Value > PerformanceTargets.MaxPageSize)
            return PerformanceTargets.MaxPageSize;

        return pageSize.Value;
    }
}

/// <summary>
/// Performance metrics for logging and monitoring
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Operation name
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Start timestamp
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End timestamp
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Operation duration
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Whether operation met performance target
    /// </summary>
    public bool MetTarget { get; set; }

    /// <summary>
    /// Target duration for this operation
    /// </summary>
    public TimeSpan? TargetDuration { get; set; }

    /// <summary>
    /// Number of records processed
    /// </summary>
    public int? RecordCount { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
