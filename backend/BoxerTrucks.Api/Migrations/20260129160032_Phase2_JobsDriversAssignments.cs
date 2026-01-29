using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoxerTrucks.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_JobsDriversAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    VehicleType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    MilesRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    HoursLow = table.Column<decimal>(type: "TEXT", nullable: false),
                    HoursHigh = table.Column<decimal>(type: "TEXT", nullable: false),
                    MilesPay = table.Column<decimal>(type: "TEXT", nullable: false),
                    PayLow = table.Column<decimal>(type: "TEXT", nullable: false),
                    PayHigh = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuoteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    CustomerPhone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    PickupAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DropoffAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ScheduledStartAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerTotalLow = table.Column<decimal>(type: "TEXT", nullable: false),
                    CustomerTotalHigh = table.Column<decimal>(type: "TEXT", nullable: false),
                    PlatformFeeLow = table.Column<decimal>(type: "TEXT", nullable: false),
                    PlatformFeeHigh = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAssignments_JobId_DriverId_Role",
                table: "JobAssignments",
                columns: new[] { "JobId", "DriverId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_QuoteId",
                table: "Jobs",
                column: "QuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "JobAssignments");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
