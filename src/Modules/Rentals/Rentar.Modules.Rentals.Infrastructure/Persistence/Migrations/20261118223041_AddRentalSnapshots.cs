using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Car_BrandId",
                schema: "rentals",
                table: "Rentals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Car_BrandName",
                schema: "rentals",
                table: "Rentals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Car_ColorId",
                schema: "rentals",
                table: "Rentals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Car_ColorName",
                schema: "rentals",
                table: "Rentals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Car_Description",
                schema: "rentals",
                table: "Rentals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Car_ModelYear",
                schema: "rentals",
                table: "Rentals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Customer_CompanyId",
                schema: "rentals",
                table: "Rentals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Customer_CompanyName",
                schema: "rentals",
                table: "Rentals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Customer_FirstName",
                schema: "rentals",
                table: "Rentals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Customer_LastName",
                schema: "rentals",
                table: "Rentals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPrice",
                schema: "rentals",
                table: "Rentals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                schema: "rentals",
                table: "Rentals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            // Rentals booked before used to show their car, user and company as those are now, so that is the best
            // snapshot a stored rental can get. Schemas migrate in alphabetical order, so the other modules' tables are
            // there already; a database where they are not keeps the defaults, hence the guards, and EXECUTE so the
            // statements are only planned when the tables exist. A rental whose car or customer's user is gone keeps
            // empty names and a price of 0.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('fleet."Cars"') IS NOT NULL
                        AND to_regclass('fleet."Brands"') IS NOT NULL
                        AND to_regclass('fleet."Colors"') IS NOT NULL
                    THEN
                        EXECUTE 'UPDATE rentals."Rentals" r
                                 SET "Car_BrandId" = c."BrandId",
                                     "Car_BrandName" = COALESCE(b."BrandName", ''''),
                                     "Car_ColorId" = c."ColorId",
                                     "Car_ColorName" = COALESCE(co."ColorName", ''''),
                                     "Car_ModelYear" = c."ModelYear",
                                     "Car_Description" = COALESCE(c."Description", ''''),
                                     "DailyPrice" = c."DailyPrice"
                                 FROM fleet."Cars" c
                                 JOIN fleet."Brands" b ON b."Id" = c."BrandId"
                                 JOIN fleet."Colors" co ON co."Id" = c."ColorId"
                                 WHERE c."Id" = r."CarId"';
                    END IF;

                    IF to_regclass('customers."Customers"') IS NOT NULL
                        AND to_regclass('identity."Users"') IS NOT NULL
                        AND to_regclass('companies."Companies"') IS NOT NULL
                    THEN
                        EXECUTE 'UPDATE rentals."Rentals" r
                                 SET "Customer_FirstName" = COALESCE(u."FirstName", ''''),
                                     "Customer_LastName" = COALESCE(u."LastName", ''''),
                                     "Customer_CompanyId" = cu."CompanyId",
                                     "Customer_CompanyName" = co."Name"
                                 FROM customers."Customers" cu
                                 JOIN identity."Users" u ON u."Id" = cu."UserId"
                                 LEFT JOIN companies."Companies" co ON co."Id" = cu."CompanyId"
                                 WHERE cu."Id" = r."CustomerId"';
                    END IF;
                END $$;
                """);

            // Like Rental does: each 24 hours that are started count as a whole day.
            migrationBuilder.Sql(
                """
                UPDATE rentals."Rentals"
                SET "TotalPrice" = "DailyPrice"
                    * GREATEST(1, CEIL(EXTRACT(EPOCH FROM ("ReturnDate" - "RentDate")) / 86400));
                """);

            // The defaults only filled the rows stored before; every rental from now on brings its own snapshot.
            migrationBuilder.Sql(
                """
                ALTER TABLE rentals."Rentals"
                    ALTER COLUMN "Car_BrandId" DROP DEFAULT,
                    ALTER COLUMN "Car_BrandName" DROP DEFAULT,
                    ALTER COLUMN "Car_ColorId" DROP DEFAULT,
                    ALTER COLUMN "Car_ColorName" DROP DEFAULT,
                    ALTER COLUMN "Car_Description" DROP DEFAULT,
                    ALTER COLUMN "Car_ModelYear" DROP DEFAULT,
                    ALTER COLUMN "Customer_FirstName" DROP DEFAULT,
                    ALTER COLUMN "Customer_LastName" DROP DEFAULT,
                    ALTER COLUMN "DailyPrice" DROP DEFAULT,
                    ALTER COLUMN "TotalPrice" DROP DEFAULT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Car_BrandId",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Car_BrandName",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Car_ColorId",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Car_ColorName",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Car_Description",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Car_ModelYear",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Customer_CompanyId",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Customer_CompanyName",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Customer_FirstName",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Customer_LastName",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DailyPrice",
                schema: "rentals",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                schema: "rentals",
                table: "Rentals");
        }
    }
}
