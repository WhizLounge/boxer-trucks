using System.ComponentModel.DataAnnotations;

namespace BoxerTrucks.Api.Models;

public class Driver
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(120)]
    public string FullName { get; set; } = "";

    [MaxLength(40)]
    public string? Phone { get; set; }

    public VehicleType VehicleType { get; set; } = VehicleType.None;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
