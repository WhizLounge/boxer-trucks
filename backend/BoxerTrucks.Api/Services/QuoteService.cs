using BoxerTrucks.Api.Data;
using BoxerTrucks.Api.Dtos;
using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Services;

public class QuoteService
{
    private readonly AppDbContext _db;

    public QuoteService(AppDbContext db)
    {
        _db = db;
    }

    // Your current pricing rules (v1)
    private const decimal DriverOvertimeRatePerHour = 75m;
    private const decimal HelperRatePerHour = 45m;

    private const decimal DriverDieselPerMile = 4.00m;
    private const decimal HelperGasPerMile = 3.00m;

    private const decimal StairsPerFlight = 25m;
    private const decimal LongCarryFee = 50m;
    private const decimal HeavyItemFeeLow = 75m;
    private const decimal HeavyItemFeeHigh = 150m;

    public QuoteResponseDto CreateEstimate(QuoteRequestDto req)
    {
        var baseRate = GetBaseFlatRate(req.HomeSize);

        // Mileage: includes going + coming (round-trip)
        var roundTripMiles = req.Miles * 2;
        var mileageFee = roundTripMiles * (DriverDieselPerMile + (req.HelperCount * HelperGasPerMile));

        var addonsLow =
            (req.StairFlights * StairsPerFlight) +
            (req.LongCarry ? LongCarryFee : 0m) +
            (req.HasHeavyItem ? HeavyItemFeeLow : 0m);

        var addonsHigh =
            (req.StairFlights * StairsPerFlight) +
            (req.LongCarry ? LongCarryFee : 0m) +
            (req.HasHeavyItem ? HeavyItemFeeHigh : 0m);

        var (overtimeLow, overtimeHigh) = GetOvertimeRangeHours(req.HomeSize, req.HelperCount);

        var overtimeRatePerHour = DriverOvertimeRatePerHour + (req.HelperCount * HelperRatePerHour);
        var overtimeCostLow = overtimeLow * overtimeRatePerHour;
        var overtimeCostHigh = overtimeHigh * overtimeRatePerHour;

        var estimatedLow = baseRate + mileageFee + addonsLow + overtimeCostLow;
        var estimatedHigh = baseRate + mileageFee + addonsHigh + overtimeCostHigh;

        // ✅ Save Quote into the database
        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            HomeSize = req.HomeSize,
            SquareFeet = req.SquareFeet,
            Miles = req.Miles,
            HelperCount = req.HelperCount,
            StairFlights = req.StairFlights,
            LongCarry = req.LongCarry,
            HasHeavyItem = req.HasHeavyItem,
            EstimatedLow = RoundToNearest5(estimatedLow),
            EstimatedHigh = RoundToNearest5(estimatedHigh),
            Status = QuoteStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        };

        _db.Quotes.Add(quote);
        _db.SaveChanges();

        return new QuoteResponseDto
        {
            QuoteId = quote.Id,
            Status = quote.Status,
            EstimatedLow = quote.EstimatedLow,
            EstimatedHigh = quote.EstimatedHigh,
            BaseFlatRate = baseRate,
            MileageFee = RoundMoney(mileageFee),
            EstimatedOvertimeHoursLow = overtimeLow,
            EstimatedOvertimeHoursHigh = overtimeHigh,
            Summary =
                $"{req.HomeSize} base: ${baseRate} | " +
                $"Mileage (round trip): ${mileageFee:0.00} ({roundTripMiles:0.0} mi) | " +
                $"Overtime: {overtimeLow:0.0}–{overtimeHigh:0.0} hrs @ ${overtimeRatePerHour}/hr | " +
                $"Add-ons: stairs({req.StairFlights}), long-carry({req.LongCarry}), heavy({req.HasHeavyItem})"
        };
    }

    private static decimal GetBaseFlatRate(HomeSizeType size) => size switch
    {
        HomeSizeType.Studio => 250m,
        HomeSizeType.OneBedroom => 325m,
        HomeSizeType.TwoBedroom => 425m,
        HomeSizeType.ThreeBedroom => 525m,
        _ => 0m
    };

    private static (decimal low, decimal high) GetOvertimeRangeHours(HomeSizeType size, int helpers)
    {
        decimal helperFactor = helpers switch
        {
            <= 0 => 1.15m,
            1 => 1.00m,
            2 => 0.90m,
            _ => 0.85m
        };

        var (baseLow, baseHigh) = size switch
        {
            HomeSizeType.Studio => (0.0m, 1.0m),
            HomeSizeType.OneBedroom => (0.5m, 1.5m),
            HomeSizeType.TwoBedroom => (1.0m, 2.5m),
            HomeSizeType.ThreeBedroom => (1.5m, 3.5m),
            _ => (1.0m, 2.0m)
        };

        return (RoundTenth(baseLow * helperFactor), RoundTenth(baseHigh * helperFactor));
    }

    private static decimal RoundMoney(decimal v) => Math.Round(v, 2);
    private static decimal RoundTenth(decimal v) => Math.Round(v, 1);

    private static decimal RoundToNearest5(decimal v)
        => Math.Ceiling(v / 5m) * 5m;
}
