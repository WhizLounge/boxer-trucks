using System.ComponentModel.DataAnnotations;
using BoxerTrucks.Api.Dtos;

namespace BoxerTrucks.Api.Models;

public class Quote
{
    [Key]
    public Guid Id { get; set; }

    public string? UserId { get; set; } // later when we add auth

    public HomeSizeType HomeSize { get; set; }

    public int SquareFeet { get; set; }

    public decimal Miles { get; set; }

    public int HelperCount { get; set; }

    public int StairFlights { get; set; }

    public bool LongCarry { get; set; }

    public bool HasHeavyItem { get; set; }

    public decimal EstimatedLow { get; set; }

    public decimal EstimatedHigh { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.PendingApproval;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
