using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class PreventOverlappingRentals : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

        // PostgreSQL range exclusions protect the invariant even when two requests both passed their prechecks.
        // Closed bounds match the availability query: sharing even one endpoint counts as overlap.
        migrationBuilder.Sql(
            """
            ALTER TABLE rentals."Rentals"
                ADD CONSTRAINT "EX_Rentals_CarPeriod"
                EXCLUDE USING gist (
                    "CarId" WITH =,
                    tsrange("RentDate", "ReturnDate", '[]') WITH &&
                ) WHERE ("Status" <> 'Cancelled');

            ALTER TABLE rentals."Rentals"
                ADD CONSTRAINT "EX_Rentals_CustomerPeriod"
                EXCLUDE USING gist (
                    "CustomerId" WITH =,
                    tsrange("RentDate", "ReturnDate", '[]') WITH &&
                ) WHERE ("Status" <> 'Cancelled');
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE rentals."Rentals" DROP CONSTRAINT "EX_Rentals_CustomerPeriod";
            ALTER TABLE rentals."Rentals" DROP CONSTRAINT "EX_Rentals_CarPeriod";
            """);

        // Extensions are database-wide and may already have been used elsewhere; do not remove btree_gist.
    }
}
