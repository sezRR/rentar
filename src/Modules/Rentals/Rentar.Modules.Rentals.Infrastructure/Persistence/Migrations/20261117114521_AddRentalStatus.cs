using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Rentals.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rentals stored before payments were taken on booking count as paid for.
            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "rentals",
                table: "Rentals",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Confirmed");

            // Only for the rows above: every new rental states its status, as the model has no default.
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "rentals",
                table: "Rentals",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldDefaultValue: "Confirmed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "rentals",
                table: "Rentals");
        }
    }
}
