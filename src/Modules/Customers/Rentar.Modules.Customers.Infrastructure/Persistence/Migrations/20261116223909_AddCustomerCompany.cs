using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Customers.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCustomerCompany : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "CompanyName",
            schema: "customers",
            table: "Customers",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AddColumn<int>(
            name: "CompanyId",
            schema: "customers",
            table: "Customers",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_CompanyId",
            schema: "customers",
            table: "Customers",
            column: "CompanyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Customers_CompanyId",
            schema: "customers",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "CompanyId",
            schema: "customers",
            table: "Customers");

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
    }
}
