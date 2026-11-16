using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Fleet.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddFleetRelationships : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ColorName",
            schema: "fleet",
            table: "Colors",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "fleet",
            table: "Cars",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ImagePath",
            schema: "fleet",
            table: "CarImages",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "BrandName",
            schema: "fleet",
            table: "Brands",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Cars_BrandId",
            schema: "fleet",
            table: "Cars",
            column: "BrandId");

        migrationBuilder.CreateIndex(
            name: "IX_Cars_ColorId",
            schema: "fleet",
            table: "Cars",
            column: "ColorId");

        migrationBuilder.CreateIndex(
            name: "IX_CarImages_CarId",
            schema: "fleet",
            table: "CarImages",
            column: "CarId");

        migrationBuilder.AddForeignKey(
            name: "FK_CarImages_Cars_CarId",
            schema: "fleet",
            table: "CarImages",
            column: "CarId",
            principalSchema: "fleet",
            principalTable: "Cars",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Cars_Brands_BrandId",
            schema: "fleet",
            table: "Cars",
            column: "BrandId",
            principalSchema: "fleet",
            principalTable: "Brands",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Cars_Colors_ColorId",
            schema: "fleet",
            table: "Cars",
            column: "ColorId",
            principalSchema: "fleet",
            principalTable: "Colors",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CarImages_Cars_CarId",
            schema: "fleet",
            table: "CarImages");

        migrationBuilder.DropForeignKey(
            name: "FK_Cars_Brands_BrandId",
            schema: "fleet",
            table: "Cars");

        migrationBuilder.DropForeignKey(
            name: "FK_Cars_Colors_ColorId",
            schema: "fleet",
            table: "Cars");

        migrationBuilder.DropIndex(
            name: "IX_Cars_BrandId",
            schema: "fleet",
            table: "Cars");

        migrationBuilder.DropIndex(
            name: "IX_Cars_ColorId",
            schema: "fleet",
            table: "Cars");

        migrationBuilder.DropIndex(
            name: "IX_CarImages_CarId",
            schema: "fleet",
            table: "CarImages");

        migrationBuilder.AlterColumn<string>(
            name: "ColorName",
            schema: "fleet",
            table: "Colors",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "fleet",
            table: "Cars",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "ImagePath",
            schema: "fleet",
            table: "CarImages",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "BrandName",
            schema: "fleet",
            table: "Brands",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");
    }
}
