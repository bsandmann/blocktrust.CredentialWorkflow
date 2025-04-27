using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJwtKeyToRsaKeyPair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtSecurityKey",
                table: "TenantEntities");

            migrationBuilder.AddColumn<string>(
                name: "JwtPrivateKey",
                table: "TenantEntities",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JwtPublicKey",
                table: "TenantEntities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtPrivateKey",
                table: "TenantEntities");

            migrationBuilder.DropColumn(
                name: "JwtPublicKey",
                table: "TenantEntities");

            migrationBuilder.AddColumn<string>(
                name: "JwtSecurityKey",
                table: "TenantEntities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
