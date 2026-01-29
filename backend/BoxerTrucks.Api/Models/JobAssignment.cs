using System.ComponentModel.DataAnnotations;

namespace BoxerTrucks.Api.Models;

public class JobAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }
    public Guid DriverId { get; set; }

    public AssignmentRole Role { get; set; }

    // Transparency: store pay breakdown
    public decimal HourlyRate { get; set; }
    public decimal MilesRate { get; set; }

    public decimal HoursLow { get; set; }
    public decimal HoursHigh { get; set; }

    public decimal MilesPay { get; set; } // round-trip miles * rate
    public decimal PayLow { get; set; }
    public decimal PayHigh { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
