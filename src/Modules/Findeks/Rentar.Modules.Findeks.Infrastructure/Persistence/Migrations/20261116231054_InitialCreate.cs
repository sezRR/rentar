using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Findeks.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "findeks");

        migrationBuilder.CreateTable(
            name: "CustomerScores",
            schema: "findeks",
            columns: table => new
            {
                CustomerId = table.Column<int>(type: "integer", nullable: false),
                FindeksRating = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerScores", x => x.CustomerId);
            });

        // Ratings used to be stored on Identity users. A database from before keeps each user's rating as the score of
        // the user's customer; users without a customer lose theirs. Schemas migrate in alphabetical order, so this runs
        // after Customers allowed one customer per user and before Identity drops the column. A new database has
        // neither table yet, hence the guard, and EXECUTE so the statement is only planned when they exist.
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF to_regclass('customers."Customers"') IS NOT NULL AND EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'identity' AND table_name = 'Users' AND column_name = 'FindeksRating')
                THEN
                    EXECUTE 'INSERT INTO findeks."CustomerScores" ("CustomerId", "FindeksRating")
                             SELECT c."Id", u."FindeksRating"
                             FROM customers."Customers" c
                             JOIN identity."Users" u ON u."Id" = c."UserId"';
                END IF;
            END $$;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CustomerScores",
            schema: "findeks");
    }
}
