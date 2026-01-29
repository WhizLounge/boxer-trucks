namespace BoxerTrucks.Api.Dtos;

public class AssignJobDto
{
    public Guid MainDriverId { get; set; }
    public List<Guid> HelperIds { get; set; } = new();
}
