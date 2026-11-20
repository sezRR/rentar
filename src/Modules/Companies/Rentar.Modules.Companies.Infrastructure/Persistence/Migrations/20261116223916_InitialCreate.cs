using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rentar.Modules.Companies.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "companies");

        migrationBuilder.CreateTable(
            name: "Companies",
            schema: "companies",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Employees",
            schema: "companies",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CompanyId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
                table.ForeignKey(
                    name: "FK_Employees_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalSchema: "companies",
                    principalTable: "Companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            schema: "companies",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CompanyId = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                IsPredefined = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
                table.ForeignKey(
                    name: "FK_Roles_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalSchema: "companies",
                    principalTable: "Companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmployeeRoles",
            schema: "companies",
            columns: table => new
            {
                EmployeeId = table.Column<int>(type: "integer", nullable: false),
                RoleId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmployeeRoles", x => new { x.EmployeeId, x.RoleId });
                table.ForeignKey(
                    name: "FK_EmployeeRoles_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "companies",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EmployeeRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "companies",
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeRoles_RoleId",
            schema: "companies",
            table: "EmployeeRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_CompanyId_UserId",
            schema: "companies",
            table: "Employees",
            columns: new[] { "CompanyId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Employees_UserId",
            schema: "companies",
            table: "Employees",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Roles_CompanyId_Name",
            schema: "companies",
            table: "Roles",
            columns: new[] { "CompanyId", "Name" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EmployeeRoles",
            schema: "companies");

        migrationBuilder.DropTable(
            name: "Employees",
            schema: "companies");

        migrationBuilder.DropTable(
            name: "Roles",
            schema: "companies");

        migrationBuilder.DropTable(
            name: "Companies",
            schema: "companies");
    }
}
