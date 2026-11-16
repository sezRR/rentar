using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class IndexRentalCar : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Rentals_CarId",
            schema: "rentals",
            table: "Rentals",
            column: "CarId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Rentals_CarId",
            schema: "rentals",
            table: "Rentals");
    }
}
