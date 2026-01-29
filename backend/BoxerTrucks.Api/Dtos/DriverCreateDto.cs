using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Dtos;

public class DriverCreateDto
{
    public string FullName { get; set; } = "";
    public string? Phone { get; set; }
    public VehicleType VehicleType { get; set; } = VehicleType.None;
}
