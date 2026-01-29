using Microsoft.EntityFrameworkCore;
using BoxerTrucks.Api.Data;
using BoxerTrucks.Api.Dtos;
using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Services;

public class JobService
{
    private readonly AppDbContext _db;
    private readonly ITimeProvider _time;

    // Rates (Phase 2 rules)
    private const decimal PlatformFeePercent = 0.08m;

    private const decimal DriverHourlyRate = 75m;
    private const decimal HelperHourlyRate = 45m;

    private const decimal DriverMilesRate = 4.00m;  // diesel
    private const decimal HelperMilesRate = 3.00m;  // gas

    private const decimal IncludedHoursForDriver = 3.0m; // base covers first 3 hrs

    public JobService(AppDbContext db, ITimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<Job> CreateJobAsync(CreateJobDto dto)
    {
        var quoteExists = await _db.Quotes.AnyAsync(q => q.Id == dto.QuoteId);
        if (!quoteExists) throw new InvalidOperationException("Quote not found.");

        var job = new Job
        {
            QuoteId = dto.QuoteId,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            PickupAddress = dto.PickupAddress.Trim(),
            DropoffAddress = dto.DropoffAddress.Trim(),
            ScheduledStartAt = dto.ScheduledStartAt,
            Status = JobStatus.PendingApproval,
            CreatedAt = _time.UtcNow
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    public async Task AssignAsync(Guid jobId, AssignJobDto dto)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null) throw new InvalidOperationException("Job not found.");

        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.Id == job.QuoteId);
        if (quote is null) throw new InvalidOperationException("Quote not found for job.");

        // Load main driver
        var main = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == dto.MainDriverId && d.IsActive);
        if (main is null) throw new InvalidOperationException("Main driver not found/active.");

        if (main.VehicleType is not (VehicleType.BoxTruck or VehicleType.Van or VehicleType.Pickup))
            throw new InvalidOperationException("Main driver must have a BoxTruck, Van, or Pickup.");

        // Load helpers
        var helperIds = dto.HelperIds.Distinct().Where(id => id != dto.MainDriverId).ToList();

        var helpers = await _db.Drivers
            .Where(d => helperIds.Contains(d.Id) && d.IsActive)
            .ToListAsync();

        if (helpers.Count != helperIds.Count)
            throw new InvalidOperationException("One or more helpers not found/active.");

        // Remove previous assignments for this job (MVP behavior)
        var old = await _db.JobAssignments.Where(a => a.JobId == jobId).ToListAsync();
        if (old.Count > 0) _db.JobAssignments.RemoveRange(old);

        // Pricing inputs
        var helperCount = helpers.Count;
        var roundTripMiles = quote.Miles * 2m;

        // Estimated hours range:
        // total hours = 3 included + overtime estimate based on home size + helper count
        var (otLow, otHigh) = GetOvertimeRangeHours(quote.HomeSize, helperCount);
        var hoursLow = IncludedHoursForDriver + otLow;
        var hoursHigh = IncludedHoursForDriver + otHigh;

        // Helpers: each helper gets hours + miles
        var helperAssignments = new List<JobAssignment>();
        foreach (var h in helpers)
        {
            var milesPay = RoundMoney(roundTripMiles * HelperMilesRate);
            var payLow = RoundMoney((hoursLow * HelperHourlyRate) + milesPay);
            var payHigh = RoundMoney((hoursHigh * HelperHourlyRate) + milesPay);

            helperAssignments.Add(new JobAssignment
            {
                JobId = jobId,
                DriverId = h.Id,
                Role = AssignmentRole.Helper,
                HourlyRate = HelperHourlyRate,
                MilesRate = HelperMilesRate,
                HoursLow = RoundTenth(hoursLow),
                HoursHigh = RoundTenth(hoursHigh),
                MilesPay = milesPay,
                PayLow = payLow,
                PayHigh = payHigh,
                CreatedAt = _time.UtcNow
            });
        }

        // Main driver pay:
        // Base + add-ons cover first 3 hours, overtime after 3 hours at $75/hr, plus miles.
        var baseRate = GetBaseFlatRate(quote.HomeSize);
        var addonsLow = GetAddonsLow(quote);
        var addonsHigh = GetAddonsHigh(quote);

        var driverMilesPay = RoundMoney(roundTripMiles * DriverMilesRate);

        var driverOvertimeLow = Math.Max(0m, hoursLow - IncludedHoursForDriver);
        var driverOvertimeHigh = Math.Max(0m, hoursHigh - IncludedHoursForDriver);

        var driverPayLow = RoundMoney(baseRate + addonsLow + driverMilesPay + (driverOvertimeLow * DriverHourlyRate));
        var driverPayHigh = RoundMoney(baseRate + addonsHigh + driverMilesPay + (driverOvertimeHigh * DriverHourlyRate));

        var mainAssignment = new JobAssignment
        {
            JobId = jobId,
            DriverId = main.Id,
            Role = AssignmentRole.MainDriver,
            HourlyRate = DriverHourlyRate,
            MilesRate = DriverMilesRate,
            HoursLow = RoundTenth(hoursLow),
            HoursHigh = RoundTenth(hoursHigh),
            MilesPay = driverMilesPay,
            PayLow = driverPayLow,
            PayHigh = driverPayHigh,
            CreatedAt = _time.UtcNow
        };

        // Total payouts (to workers) before platform fee
        var payoutsLow = driverPayLow + helperAssignments.Sum(x => x.PayLow);
        var payoutsHigh = driverPayHigh + helperAssignments.Sum(x => x.PayHigh);

        // Customer totals include platform fee. “Gross up” so worker payouts stay fair.
        var customerLow = RoundToNearest5(payoutsLow / (1m - PlatformFeePercent));
        var customerHigh = RoundToNearest5(payoutsHigh / (1m - PlatformFeePercent));

        var platformFeeLow = RoundMoney(customerLow - payoutsLow);
        var platformFeeHigh = RoundMoney(customerHigh - payoutsHigh);

        job.CustomerTotalLow = customerLow;
        job.CustomerTotalHigh = customerHigh;
        job.PlatformFeeLow = platformFeeLow;
        job.PlatformFeeHigh = platformFeeHigh;
        job.Status = JobStatus.Assigned;

        _db.JobAssignments.Add(mainAssignment);
        _db.JobAssignments.AddRange(helperAssignments);

        await _db.SaveChangesAsync();
    }

    public async Task StartAsync(Guid jobId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null) throw new InvalidOperationException("Job not found.");

        if (job.Status != JobStatus.Assigned && job.Status != JobStatus.Approved)
            throw new InvalidOperationException("Job must be assigned/approved before starting.");

        job.StartedAt = _time.UtcNow;
        job.Status = JobStatus.InProgress;
        await _db.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid jobId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null) throw new InvalidOperationException("Job not found.");

        if (job.Status != JobStatus.InProgress)
            throw new InvalidOperationException("Job must be in progress before completing.");

        if (job.StartedAt is null)
            throw new InvalidOperationException("Job has no start time.");

        job.CompletedAt = _time.UtcNow;
        job.Status = JobStatus.Completed;

        // Recalculate payouts using ACTUAL hours (server time)
        var durationHours = (decimal)(job.CompletedAt.Value - job.StartedAt.Value).TotalMinutes / 60m;
        var actualHours = RoundUpToQuarterHour(durationHours);

        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.Id == job.QuoteId);
        if (quote is null) throw new InvalidOperationException("Quote not found for job.");

        var assignments = await _db.JobAssignments.Where(a => a.JobId == jobId).ToListAsync();
        if (assignments.Count == 0)
        {
            await _db.SaveChangesAsync();
            return;
        }

        var roundTripMiles = quote.Miles * 2m;

        // Update helper pay based on actual hours
        foreach (var a in assignments.Where(x => x.Role == AssignmentRole.Helper))
        {
            a.HoursLow = RoundTenth(actualHours);
            a.HoursHigh = RoundTenth(actualHours);

            a.HourlyRate = HelperHourlyRate;
            a.MilesRate = HelperMilesRate;

            a.MilesPay = RoundMoney(roundTripMiles * HelperMilesRate);

            var pay = RoundMoney((actualHours * HelperHourlyRate) + a.MilesPay);
            a.PayLow = pay;
            a.PayHigh = pay;
        }

        // Update main driver pay: overtime after first 3 hours @ $75/hr
        var main = assignments.First(x => x.Role == AssignmentRole.MainDriver);

        var baseRate = GetBaseFlatRate(quote.HomeSize);
        var addonsHigh = GetAddonsHigh(quote); // use high to be safe for final

        var driverMilesPay = RoundMoney(roundTripMiles * DriverMilesRate);
        var overtimeHours = Math.Max(0m, actualHours - IncludedHoursForDriver);

        main.HoursLow = RoundTenth(actualHours);
        main.HoursHigh = RoundTenth(actualHours);

        main.HourlyRate = DriverHourlyRate;
        main.MilesRate = DriverMilesRate;

        main.MilesPay = driverMilesPay;

        var driverPay = RoundMoney(baseRate + addonsHigh + driverMilesPay + (overtimeHours * DriverHourlyRate));
        main.PayLow = driverPay;
        main.PayHigh = driverPay;

        // Recompute customer totals (single actual number now)
        var totalPayouts = assignments.Sum(x => x.PayHigh);
        var customerTotal = RoundToNearest5(totalPayouts / (1m - PlatformFeePercent));
        var platformFee = RoundMoney(customerTotal - totalPayouts);

        job.CustomerTotalLow = customerTotal;
        job.CustomerTotalHigh = customerTotal;
        job.PlatformFeeLow = platformFee;
        job.PlatformFeeHigh = platformFee;

        await _db.SaveChangesAsync();
    }

    public async Task<JobDetailsDto> GetDetailsAsync(Guid jobId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null) throw new InvalidOperationException("Job not found.");

        var assignments = await _db.JobAssignments.Where(a => a.JobId == jobId).ToListAsync();
        var driverIds = assignments.Select(a => a.DriverId).Distinct().ToList();

        var drivers = await _db.Drivers.Where(d => driverIds.Contains(d.Id)).ToListAsync();
        var nameMap = drivers.ToDictionary(d => d.Id, d => d.FullName);

        return new JobDetailsDto
        {
            JobId = job.Id,
            QuoteId = job.QuoteId,
            Status = job.Status,
            CustomerName = job.CustomerName,
            CustomerPhone = job.CustomerPhone,
            PickupAddress = job.PickupAddress,
            DropoffAddress = job.DropoffAddress,
            ScheduledStartAt = job.ScheduledStartAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            CustomerTotalLow = job.CustomerTotalLow,
            CustomerTotalHigh = job.CustomerTotalHigh,
            PlatformFeeLow = job.PlatformFeeLow,
            PlatformFeeHigh = job.PlatformFeeHigh,
            Assignments = assignments.Select(a => new JobAssignmentDto
            {
                DriverId = a.DriverId,
                DriverName = nameMap.TryGetValue(a.DriverId, out var n) ? n : "",
                Role = a.Role,
                HourlyRate = a.HourlyRate,
                MilesRate = a.MilesRate,
                HoursLow = a.HoursLow,
                HoursHigh = a.HoursHigh,
                MilesPay = a.MilesPay,
                PayLow = a.PayLow,
                PayHigh = a.PayHigh
            }).ToList()
        };
    }

    // ---------- helpers ----------

    private static decimal GetBaseFlatRate(HomeSizeType size) => size switch
    {
        HomeSizeType.Studio => 250m,
        HomeSizeType.OneBedroom => 325m,
        HomeSizeType.TwoBedroom => 425m,
        HomeSizeType.ThreeBedroom => 525m,
        _ => 0m
    };

    private static decimal GetAddonsLow(Quote q)
        => (q.StairFlights * 25m)
           + (q.LongCarry ? 50m : 0m)
           + (q.HasHeavyItem ? 75m : 0m);

    private static decimal GetAddonsHigh(Quote q)
        => (q.StairFlights * 25m)
           + (q.LongCarry ? 50m : 0m)
           + (q.HasHeavyItem ? 150m : 0m);

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
    private static decimal RoundToNearest5(decimal v) => Math.Ceiling(v / 5m) * 5m;

    private static decimal RoundUpToQuarterHour(decimal hours)
    {
        var quarters = Math.Ceiling(hours / 0.25m);
        return quarters * 0.25m;
    }
}
