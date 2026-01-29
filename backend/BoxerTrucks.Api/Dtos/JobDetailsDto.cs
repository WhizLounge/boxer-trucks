using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Dtos;

public class JobDetailsDto
{
    public Guid JobId { get; set; }
    public Guid QuoteId { get; set; }
    public JobStatus Status { get; set; }

    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string PickupAddress { get; set; } = "";
    public string DropoffAddress { get; set; } = "";

    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public decimal CustomerTotalLow { get; set; }
    public decimal CustomerTotalHigh { get; set; }
    public decimal PlatformFeeLow { get; set; }
    public decimal PlatformFeeHigh { get; set; }

    public List<JobAssignmentDto> Assignments { get; set; } = new();
}

public class JobAssignmentDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = "";
    public AssignmentRole Role { get; set; }

    public decimal HourlyRate { get; set; }
    public decimal MilesRate { get; set; }

    public decimal HoursLow { get; set; }
    public decimal HoursHigh { get; set; }

    public decimal MilesPay { get; set; }
    public decimal PayLow { get; set; }
    public decimal PayHigh { get; set; }
}
