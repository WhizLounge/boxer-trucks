using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoxerTrucks.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    HomeSize = table.Column<int>(type: "INTEGER", nullable: false),
                    SquareFeet = table.Column<int>(type: "INTEGER", nullable: false),
                    Miles = table.Column<decimal>(type: "TEXT", nullable: false),
                    HelperCount = table.Column<int>(type: "INTEGER", nullable: false),
                    StairFlights = table.Column<int>(type: "INTEGER", nullable: false),
                    LongCarry = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasHeavyItem = table.Column<bool>(type: "INTEGER", nullable: false),
                    EstimatedLow = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedHigh = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quotes");
        }
    }
}
