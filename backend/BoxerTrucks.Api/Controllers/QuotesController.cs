using Microsoft.AspNetCore.Mvc;
using BoxerTrucks.Api.Dtos;
using BoxerTrucks.Api.Services;

namespace BoxerTrucks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly QuoteService _quoteService;

    public QuotesController(QuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpPost("estimate")]
    public ActionResult<QuoteResponseDto> CreateEstimate([FromBody] QuoteRequestDto req)
    {
        if (req.Miles < 0) return BadRequest("Miles cannot be negative.");
        if (req.SquareFeet < 0) return BadRequest("SquareFeet cannot be negative.");
        if (req.HelperCount < 0) return BadRequest("HelperCount cannot be negative.");
        if (req.StairFlights < 0) return BadRequest("StairFlights cannot be negative.");

        var result = _quoteService.CreateEstimate(req);
        return Ok(result);
    }
}
