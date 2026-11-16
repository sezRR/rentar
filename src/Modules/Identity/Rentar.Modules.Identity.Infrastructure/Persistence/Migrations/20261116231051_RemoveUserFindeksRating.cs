using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentar.Modules.Identity.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class RemoveUserFindeksRating : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FindeksRating",
            schema: "identity",
            table: "Users");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FindeksRating",
            schema: "identity",
            table: "Users",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
