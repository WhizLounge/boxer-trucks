using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Dtos;

public class DriverResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string? Phone { get; set; }
    public VehicleType VehicleType { get; set; }
    public bool IsActive { get; set; }
}
