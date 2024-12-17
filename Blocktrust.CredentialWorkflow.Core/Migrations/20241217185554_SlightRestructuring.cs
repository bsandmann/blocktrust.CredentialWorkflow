using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class SlightRestructuring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssuingKey_TenantEntities_TenantEntityId",
                table: "IssuingKey");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssuingKey",
                table: "IssuingKey");

            migrationBuilder.RenameTable(
                name: "IssuingKey",
                newName: "IssuingKeys");

            migrationBuilder.RenameIndex(
                name: "IX_IssuingKey_TenantEntityId",
                table: "IssuingKeys",
                newName: "IX_IssuingKeys_TenantEntityId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantEntityId",
                table: "IssuingKeys",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssuingKeys",
                table: "IssuingKeys",
                column: "IssuingKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuingKeys_TenantEntities_TenantEntityId",
                table: "IssuingKeys",
                column: "TenantEntityId",
                principalTable: "TenantEntities",
                principalColumn: "TenantEntityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssuingKeys_TenantEntities_TenantEntityId",
                table: "IssuingKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssuingKeys",
                table: "IssuingKeys");

            migrationBuilder.RenameTable(
                name: "IssuingKeys",
                newName: "IssuingKey");

            migrationBuilder.RenameIndex(
                name: "IX_IssuingKeys_TenantEntityId",
                table: "IssuingKey",
                newName: "IX_IssuingKey_TenantEntityId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantEntityId",
                table: "IssuingKey",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssuingKey",
                table: "IssuingKey",
                column: "IssuingKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuingKey_TenantEntities_TenantEntityId",
                table: "IssuingKey",
                column: "TenantEntityId",
                principalTable: "TenantEntities",
                principalColumn: "TenantEntityId");
        }
    }
}
