using System.ComponentModel.DataAnnotations;

namespace BoxerTrucks.Api.Models;

public class Job
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Link to Phase 1 Quote
    public Guid QuoteId { get; set; }

    // Simple MVP customer fields (replace with auth later)
    [MaxLength(120)]
    public string CustomerName { get; set; } = "";

    [MaxLength(40)]
    public string CustomerPhone { get; set; } = "";

    [MaxLength(200)]
    public string PickupAddress { get; set; } = "";

    [MaxLength(200)]
    public string DropoffAddress { get; set; } = "";

    // Scheduling requested by customer
    public DateTime? ScheduledStartAt { get; set; }

    // Actual job clock (server-controlled)
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public JobStatus Status { get; set; } = JobStatus.PendingApproval;

    // Optional snapshot fields for reporting
    public decimal CustomerTotalLow { get; set; }
    public decimal CustomerTotalHigh { get; set; }
    public decimal PlatformFeeLow { get; set; }
    public decimal PlatformFeeHigh { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
