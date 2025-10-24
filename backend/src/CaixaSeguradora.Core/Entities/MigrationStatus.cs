using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Migration Status - Overall project tracking (New table: MIGRATION_STATUS)
/// Tracks migration progress for each user story including requirements, tests, and timeline.
/// </summary>
[Table("MIGRATION_STATUS")]
[Index(nameof(UserStoryCode), Name = "IX_MIGRATION_STATUS_Code", IsUnique = true)]
[Index(nameof(Status), nameof(EstimatedCompletion), Name = "IX_MIGRATION_STATUS_StatusDate")]
public class MigrationStatus
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    /// <summary>
    /// User Story Code - Unique identifier for user story (e.g., "US-001")
    /// </summary>
    [Column("USER_STORY_CODE")]
    [Required]
    [StringLength(20)]
    public string UserStoryCode { get; set; } = string.Empty;

    /// <summary>
    /// User Story Name - Descriptive name
    /// </summary>
    [Column("USER_STORY_NAME")]
    [Required]
    [StringLength(200)]
    public string UserStoryName { get; set; } = string.Empty;

    /// <summary>
    /// Current Status - Not Started, In Progress, Completed, Blocked
    /// </summary>
    [Column("STATUS")]
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Not Started";

    /// <summary>
    /// Completion Percentage (0-100)
    /// </summary>
    [Column("COMPLETION_PERCENTAGE", TypeName = "decimal(5,2)")]
    [Required]
    [Range(0, 100)]
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Number of Functional Requirements Completed
    /// </summary>
    [Column("REQUIREMENTS_COMPLETED")]
    [Required]
    [Range(0, int.MaxValue)]
    public int RequirementsCompleted { get; set; }

    /// <summary>
    /// Total Number of Functional Requirements
    /// </summary>
    [Column("REQUIREMENTS_TOTAL")]
    [Required]
    [Range(1, int.MaxValue)]
    public int RequirementsTotal { get; set; }

    /// <summary>
    /// Number of Tests Passed
    /// </summary>
    [Column("TESTS_PASSED")]
    [Required]
    [Range(0, int.MaxValue)]
    public int TestsPassed { get; set; }

    /// <summary>
    /// Total Number of Tests
    /// </summary>
    [Column("TESTS_TOTAL")]
    [Required]
    [Range(0, int.MaxValue)]
    public int TestsTotal { get; set; }

    /// <summary>
    /// Assigned Team Member
    /// </summary>
    [Column("ASSIGNED_TO")]
    [StringLength(100)]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Start Date
    /// </summary>
    [Column("START_DATE")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Estimated Completion Date
    /// </summary>
    [Column("ESTIMATED_COMPLETION")]
    public DateTime? EstimatedCompletion { get; set; }

    /// <summary>
    /// Actual Completion Date
    /// </summary>
    [Column("ACTUAL_COMPLETION")]
    public DateTime? ActualCompletion { get; set; }

    /// <summary>
    /// Blocking Issues (if status is Blocked)
    /// </summary>
    [Column("BLOCKING_ISSUES")]
    [StringLength(1000)]
    public string? BlockingIssues { get; set; }

    /// <summary>
    /// Priority Level (P1-P6)
    /// </summary>
    [Column("PRIORITY")]
    [StringLength(10)]
    public string? Priority { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Related component migrations
    /// </summary>
    public virtual ICollection<ComponentMigration> Components { get; set; } = new List<ComponentMigration>();

    // Computed Properties
    /// <summary>
    /// Test pass rate percentage
    /// </summary>
    [NotMapped]
    public decimal TestPassRate
    {
        get
        {
            if (TestsTotal == 0)
                return 0;

            return Math.Round((decimal)TestsPassed / TestsTotal * 100, 2);
        }
    }

    /// <summary>
    /// Requirements completion rate percentage
    /// </summary>
    [NotMapped]
    public decimal RequirementsCompletionRate
    {
        get
        {
            if (RequirementsTotal == 0)
                return 0;

            return Math.Round((decimal)RequirementsCompleted / RequirementsTotal * 100, 2);
        }
    }

    /// <summary>
    /// Days remaining until estimated completion (negative if overdue)
    /// </summary>
    [NotMapped]
    public int? DaysRemaining
    {
        get
        {
            if (!EstimatedCompletion.HasValue || ActualCompletion.HasValue)
                return null;

            return (EstimatedCompletion.Value.Date - DateTime.Today).Days;
        }
    }

    /// <summary>
    /// Is user story overdue
    /// </summary>
    [NotMapped]
    public bool IsOverdue
    {
        get
        {
            if (!EstimatedCompletion.HasValue || ActualCompletion.HasValue)
                return false;

            return DateTime.Today > EstimatedCompletion.Value.Date && Status != "Completed";
        }
    }

    /// <summary>
    /// Is user story blocked
    /// </summary>
    [NotMapped]
    public bool IsBlocked => Status == "Blocked";
}
