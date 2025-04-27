using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddJwtSecurityKeyToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JwtSecurityKey",
                table: "TenantEntities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtSecurityKey",
                table: "TenantEntities");
        }
    }
}
