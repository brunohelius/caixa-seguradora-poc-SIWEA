namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Dashboard overview with migration progress metrics
/// </summary>
public class DashboardOverviewDto
{
    /// <summary>
    /// Migration progress percentage (0-100)
    /// </summary>
    public decimal ProgressPercentage { get; set; }

    /// <summary>
    /// Total tasks count
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Completed tasks count
    /// </summary>
    public int CompletedTasks { get; set; }

    /// <summary>
    /// In-progress tasks count
    /// </summary>
    public int InProgressTasks { get; set; }

    /// <summary>
    /// Pending tasks count
    /// </summary>
    public int PendingTasks { get; set; }

    /// <summary>
    /// Total user stories count
    /// </summary>
    public int TotalUserStories { get; set; }

    /// <summary>
    /// Completed user stories count
    /// </summary>
    public int CompletedUserStories { get; set; }

    /// <summary>
    /// Total migrated claims count
    /// </summary>
    public int TotalClaimsMigrated { get; set; }

    /// <summary>
    /// Claims processed today
    /// </summary>
    public int ClaimsProcessedToday { get; set; }

    /// <summary>
    /// Average processing time in milliseconds
    /// </summary>
    public long AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Success rate percentage (0-100)
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// System uptime in seconds
    /// </summary>
    public long UptimeSeconds { get; set; }

    /// <summary>
    /// Last deployment timestamp
    /// </summary>
    public DateTime? LastDeploymentAt { get; set; }

    /// <summary>
    /// Next milestone date
    /// </summary>
    public DateTime? NextMilestoneDate { get; set; }

    /// <summary>
    /// Next milestone description
    /// </summary>
    public string? NextMilestoneDescription { get; set; }

    /// <summary>
    /// Critical issues count
    /// </summary>
    public int CriticalIssuesCount { get; set; }

    /// <summary>
    /// Warnings count
    /// </summary>
    public int WarningsCount { get; set; }
}

/// <summary>
/// Component status DTO
/// </summary>
public class ComponentStatusDto
{
    /// <summary>
    /// Component/user story name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Component identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Status (COMPLETED, IN_PROGRESS, PENDING, BLOCKED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public decimal ProgressPercentage { get; set; }

    /// <summary>
    /// Tasks completed count
    /// </summary>
    public int TasksCompleted { get; set; }

    /// <summary>
    /// Total tasks count
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Priority (HIGH, MEDIUM, LOW)
    /// </summary>
    public string Priority { get; set; } = "MEDIUM";

    /// <summary>
    /// Start date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Assigned team member
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Performance metric DTO
/// </summary>
public class PerformanceMetricDto
{
    /// <summary>
    /// Metric date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Metric type (API_RESPONSE_TIME, THROUGHPUT, ERROR_RATE, etc.)
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Metric value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Metric unit (ms, requests/sec, %, etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Target/baseline value
    /// </summary>
    public decimal? TargetValue { get; set; }

    /// <summary>
    /// Additional metric data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Activity DTO
/// </summary>
public class ActivityDto
{
    /// <summary>
    /// Activity unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Activity type (DEPLOYMENT, TASK_COMPLETED, ERROR, WARNING, INFO)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Activity title/summary
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Activity description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// User who triggered the activity
    /// </summary>
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// Activity timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Severity (INFO, WARNING, ERROR, CRITICAL)
    /// </summary>
    public string Severity { get; set; } = "INFO";

    /// <summary>
    /// Related entity ID (claim ID, task ID, etc.)
    /// </summary>
    public string? RelatedEntityId { get; set; }

    /// <summary>
    /// Related entity type (CLAIM, TASK, DEPLOYMENT, etc.)
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Additional activity metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// System health DTO
/// </summary>
public class SystemHealthDto
{
    /// <summary>
    /// Overall system status (HEALTHY, DEGRADED, UNHEALTHY)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Database health status
    /// </summary>
    public string DatabaseStatus { get; set; } = string.Empty;

    /// <summary>
    /// Database response time in milliseconds
    /// </summary>
    public long DatabaseResponseTimeMs { get; set; }

    /// <summary>
    /// External services health
    /// </summary>
    public Dictionary<string, string> ExternalServicesStatus { get; set; } = new();

    /// <summary>
    /// Memory usage percentage (0-100)
    /// </summary>
    public decimal MemoryUsagePercentage { get; set; }

    /// <summary>
    /// CPU usage percentage (0-100)
    /// </summary>
    public decimal CpuUsagePercentage { get; set; }

    /// <summary>
    /// Active requests count
    /// </summary>
    public int ActiveRequestsCount { get; set; }

    /// <summary>
    /// Requests per minute
    /// </summary>
    public int RequestsPerMinute { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public long AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime LastCheckAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Health check details
    /// </summary>
    public List<HealthCheckDetailDto> HealthChecks { get; set; } = new();
}

/// <summary>
/// Health check detail DTO
/// </summary>
public class HealthCheckDetailDto
{
    /// <summary>
    /// Health check name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Health check status (HEALTHY, DEGRADED, UNHEALTHY)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Error message (if unhealthy)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional health check data
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}
