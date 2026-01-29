using Microsoft.EntityFrameworkCore;
using BoxerTrucks.Api.Models;

namespace BoxerTrucks.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Quote> Quotes => Set<Quote>();
}
