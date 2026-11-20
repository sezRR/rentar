using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class IndexRentalCustomer : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Rentals_CustomerId",
            schema: "rentals",
            table: "Rentals",
            column: "CustomerId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Rentals_CustomerId",
            schema: "rentals",
            table: "Rentals");
    }
}
