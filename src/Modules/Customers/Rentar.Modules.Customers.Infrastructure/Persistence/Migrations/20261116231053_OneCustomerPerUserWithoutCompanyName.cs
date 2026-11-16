using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Customers.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class OneCustomerPerUserWithoutCompanyName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Customers_UserId",
            schema: "customers",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "CompanyName",
            schema: "customers",
            table: "Customers");

        // A user used to be able to have several customers. Each user's oldest customer is kept: it takes the company of a
        // newer one when it has none, the rentals of the newer ones move to it, and the newer ones are deleted. Rentals
        // live in another module's schema, which a new database does not have yet when this runs, hence the guard and
        // EXECUTE, so the statement is only planned when the table exists.
        migrationBuilder.Sql(
            """
            CREATE TEMPORARY TABLE customer_merges ON COMMIT DROP AS
            SELECT c."Id" AS "DuplicateId", k."Id" AS "KeptId", c."CompanyId"
            FROM customers."Customers" c
            JOIN customers."Customers" k
              ON k."UserId" = c."UserId"
             AND k."Id" = (SELECT min(o."Id") FROM customers."Customers" o WHERE o."UserId" = c."UserId")
            WHERE c."Id" <> k."Id";

            UPDATE customers."Customers" k
            SET "CompanyId" = (SELECT m."CompanyId" FROM customer_merges m
                               WHERE m."KeptId" = k."Id" AND m."CompanyId" IS NOT NULL
                               ORDER BY m."DuplicateId" LIMIT 1)
            WHERE k."CompanyId" IS NULL
              AND EXISTS (SELECT 1 FROM customer_merges m WHERE m."KeptId" = k."Id" AND m."CompanyId" IS NOT NULL);

            DO $$
            BEGIN
                IF to_regclass('rentals."Rentals"') IS NOT NULL THEN
                    EXECUTE 'UPDATE rentals."Rentals" r SET "CustomerId" = m."KeptId"
                             FROM customer_merges m WHERE r."CustomerId" = m."DuplicateId"';
                END IF;
            END $$;

            DELETE FROM customers."Customers" c USING customer_merges m WHERE c."Id" = m."DuplicateId";
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_UserId",
            schema: "customers",
            table: "Customers",
            column: "UserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Customers_UserId",
            schema: "customers",
            table: "Customers");

        migrationBuilder.AddColumn<string>(
            name: "CompanyName",
            schema: "customers",
            table: "Customers",
            type: "text",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_UserId",
            schema: "customers",
            table: "Customers",
            column: "UserId");
    }
}
