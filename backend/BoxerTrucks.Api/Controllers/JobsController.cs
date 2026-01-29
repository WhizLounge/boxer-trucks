using Microsoft.AspNetCore.Mvc;
using BoxerTrucks.Api.Dtos;
using BoxerTrucks.Api.Services;

namespace BoxerTrucks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly JobService _jobs;

    public JobsController(JobService jobs)
    {
        _jobs = jobs;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateJobDto dto)
    {
        var job = await _jobs.CreateJobAsync(dto);
        return Ok(new { jobId = job.Id, status = job.Status });
    }

    [HttpPost("{jobId:guid}/assign")]
    public async Task<IActionResult> Assign(Guid jobId, AssignJobDto dto)
    {
        await _jobs.AssignAsync(jobId, dto);
        return Ok(new { ok = true });
    }

    [HttpPost("{jobId:guid}/start")]
    public async Task<IActionResult> Start(Guid jobId)
    {
        await _jobs.StartAsync(jobId);
        return Ok(new { ok = true });
    }

    [HttpPost("{jobId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid jobId)
    {
        await _jobs.CompleteAsync(jobId);
        return Ok(new { ok = true });
    }

    [HttpGet("{jobId:guid}")]
    public async Task<IActionResult> Details(Guid jobId)
    {
        var details = await _jobs.GetDetailsAsync(jobId);
        return Ok(details);
    }
}
