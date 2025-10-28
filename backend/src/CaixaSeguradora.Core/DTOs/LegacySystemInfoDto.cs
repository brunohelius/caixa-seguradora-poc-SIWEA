namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// DTO containing comprehensive information about the legacy SIWEA system
/// Used for the legacy system documentation dashboard
/// </summary>
public class LegacySystemInfoDto
{
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string SystemType { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string CodeSize { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public int YearsInProduction { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public string OriginalProgrammer { get; set; } = string.Empty;
    public string OriginalAnalyst { get; set; } = string.Empty;
    public string LastMaintenance { get; set; } = string.Empty;
}

/// <summary>
/// DTO containing system usage statistics
/// </summary>
public class SystemStatisticsDto
{
    public int ActiveUsers { get; set; }
    public int DailyTransactions { get; set; }
    public decimal TotalDataVolume { get; set; }
    public string DataVolumeUnit { get; set; } = "M"; // M for millions
    public string CriticalityLevel { get; set; } = string.Empty;
    public decimal AverageResponseTime { get; set; }
    public decimal Availability { get; set; }
}

/// <summary>
/// DTO for documentation metrics
/// </summary>
public class DocumentationMetricsDto
{
    public int TotalDocuments { get; set; }
    public int TotalPages { get; set; }
    public int BusinessRules { get; set; }
    public int DatabaseTables { get; set; }
    public int ExternalIntegrations { get; set; }
    public int ScreenMaps { get; set; }
    public int WorkflowPhases { get; set; }
    public int DataStructures { get; set; }
}

/// <summary>
/// DTO for individual documentation document
/// </summary>
public class DocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Pages { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

/// <summary>
/// DTO for technology stack information
/// </summary>
public class TechnologyStackDto
{
    public List<string> LegacyTechnologies { get; set; } = new();
    public List<string> MigrationTechnologies { get; set; } = new();
}

/// <summary>
/// DTO for business processes supported by the system
/// </summary>
public class BusinessProcessDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public string PerformanceTarget { get; set; } = string.Empty;
}

/// <summary>
/// Main DTO aggregating all legacy system dashboard data
/// </summary>
public class LegacySystemDashboardDto
{
    public LegacySystemInfoDto SystemInfo { get; set; } = new();
    public SystemStatisticsDto Statistics { get; set; } = new();
    public DocumentationMetricsDto Documentation { get; set; } = new();
    public List<DocumentDto> AvailableDocuments { get; set; } = new();
    public TechnologyStackDto TechnologyStack { get; set; } = new();
    public List<BusinessProcessDto> BusinessProcesses { get; set; } = new();
    public DateTime LastAnalyzed { get; set; }
}
