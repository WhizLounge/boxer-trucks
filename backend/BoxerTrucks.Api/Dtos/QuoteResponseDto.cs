namespace BoxerTrucks.Api.Dtos;

public enum QuoteStatus
{
    PendingApproval
}

public class QuoteResponseDto
{
    public Guid QuoteId { get; set; }

    public QuoteStatus Status { get; set; }

    public decimal EstimatedLow { get; set; }

    public decimal EstimatedHigh { get; set; }

    public decimal BaseFlatRate { get; set; }

    public decimal MileageFee { get; set; }

    public decimal EstimatedOvertimeHoursLow { get; set; }

    public decimal EstimatedOvertimeHoursHigh { get; set; }

    public string Summary { get; set; } = "";
}
