namespace BoxerTrucks.Api.Dtos;

public class CreateJobDto
{
    public Guid QuoteId { get; set; }

    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";

    public string PickupAddress { get; set; } = "";
    public string DropoffAddress { get; set; } = "";

    public DateTime? ScheduledStartAt { get; set; }
}
