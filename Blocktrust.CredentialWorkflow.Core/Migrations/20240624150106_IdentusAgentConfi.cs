using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class IdentusAgentConfi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentusAgents",
                columns: table => new
                {
                    IdentusAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Uri = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentusAgents", x => x.IdentusAgentId);
                    table.ForeignKey(
                        name: "FK_IdentusAgents_TenantEntities_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantEntities",
                        principalColumn: "TenantEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentusAgents_TenantId",
                table: "IdentusAgents",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentusAgents");
        }
    }
}
