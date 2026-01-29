namespace BoxerTrucks.Api.Services;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
}
