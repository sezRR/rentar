using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Customers.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCustomerConstraints : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "CompanyName",
            schema: "customers",
            table: "Customers",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_UserId",
            schema: "customers",
            table: "Customers",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Customers_UserId",
            schema: "customers",
            table: "Customers");

        migrationBuilder.AlterColumn<string>(
            name: "CompanyName",
            schema: "customers",
            table: "Customers",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");
    }
}
