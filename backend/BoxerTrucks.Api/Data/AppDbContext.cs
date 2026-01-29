using Microsoft.EntityFrameworkCore;
using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Quote> Quotes => Set<Quote>();     // you already have this
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>()
            .HasIndex(j => j.QuoteId)
            .IsUnique(false);

        modelBuilder.Entity<JobAssignment>()
            .HasIndex(a => new { a.JobId, a.DriverId, a.Role })
            .IsUnique(false);
    }
}
