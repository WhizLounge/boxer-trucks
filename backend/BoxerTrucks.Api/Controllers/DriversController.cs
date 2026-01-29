using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxerTrucks.Api.Data;
using BoxerTrucks.Api.Dtos;
using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly AppDbContext _db;

    public DriversController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<DriverResponseDto>> Create(DriverCreateDto dto)
    {
        var d = new Driver
        {
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone?.Trim(),
            VehicleType = dto.VehicleType,
            IsActive = true
        };

        _db.Drivers.Add(d);
        await _db.SaveChangesAsync();

        return Ok(new DriverResponseDto
        {
            Id = d.Id,
            FullName = d.FullName,
            Phone = d.Phone,
            VehicleType = d.VehicleType,
            IsActive = d.IsActive
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<DriverResponseDto>>> List()
    {
        var drivers = await _db.Drivers
            .OrderBy(d => d.FullName)
            .Select(d => new DriverResponseDto
            {
                Id = d.Id,
                FullName = d.FullName,
                Phone = d.Phone,
                VehicleType = d.VehicleType,
                IsActive = d.IsActive
            })
            .ToListAsync();

        return Ok(drivers);
    }
}
