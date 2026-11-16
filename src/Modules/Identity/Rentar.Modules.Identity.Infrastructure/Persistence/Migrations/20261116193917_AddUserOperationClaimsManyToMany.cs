using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rentar.Modules.Identity.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddUserOperationClaimsManyToMany : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_UserOperationClaims",
            schema: "identity",
            table: "UserOperationClaims");

        migrationBuilder.DropColumn(
            name: "Id",
            schema: "identity",
            table: "UserOperationClaims");

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserOperationClaims",
            schema: "identity",
            table: "UserOperationClaims",
            columns: new[] { "UserId", "OperationClaimId" });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            schema: "identity",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserOperationClaims_OperationClaimId",
            schema: "identity",
            table: "UserOperationClaims",
            column: "OperationClaimId");

        migrationBuilder.CreateIndex(
            name: "IX_OperationClaims_Name",
            schema: "identity",
            table: "OperationClaims",
            column: "Name",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_UserOperationClaims_OperationClaims_OperationClaimId",
            schema: "identity",
            table: "UserOperationClaims",
            column: "OperationClaimId",
            principalSchema: "identity",
            principalTable: "OperationClaims",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_UserOperationClaims_Users_UserId",
            schema: "identity",
            table: "UserOperationClaims",
            column: "UserId",
            principalSchema: "identity",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserOperationClaims_OperationClaims_OperationClaimId",
            schema: "identity",
            table: "UserOperationClaims");

        migrationBuilder.DropForeignKey(
            name: "FK_UserOperationClaims_Users_UserId",
            schema: "identity",
            table: "UserOperationClaims");

        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            schema: "identity",
            table: "Users");

        migrationBuilder.DropPrimaryKey(
            name: "PK_UserOperationClaims",
            schema: "identity",
            table: "UserOperationClaims");

        migrationBuilder.DropIndex(
            name: "IX_UserOperationClaims_OperationClaimId",
            schema: "identity",
            table: "UserOperationClaims");

        migrationBuilder.DropIndex(
            name: "IX_OperationClaims_Name",
            schema: "identity",
            table: "OperationClaims");

        migrationBuilder.AddColumn<int>(
            name: "Id",
            schema: "identity",
            table: "UserOperationClaims",
            type: "integer",
            nullable: false,
            defaultValue: 0)
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserOperationClaims",
            schema: "identity",
            table: "UserOperationClaims",
            column: "Id");
    }
}
