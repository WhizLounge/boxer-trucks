using BoxerTrucks.Api.Services;
using Microsoft.EntityFrameworkCore;
using BoxerTrucks.Api.Data;




var builder = WebApplication.CreateBuilder(args);

// 1) Register Controllers (tells .NET you're using MVC Controllers)
builder.Services.AddControllers();

// 2) Swagger = API documentation UI (for testing endpoints)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3) Register your business logic service (Dependency Injection)

builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=boxertrucks.db");
});

var app = builder.Build();

// 4) Enable Swagger only in Development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5) Redirect HTTP → HTTPS (security)
app.UseHttpsRedirection();

// 6) Map controller routes (activates /api/... endpoints)
app.MapControllers();

// 7) Start the web server
app.Run();
