using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class peerdids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssuingKeys_TenantEntities_TenantEntityId",
                table: "IssuingKeys");

            migrationBuilder.CreateTable(
                name: "PeerDIDEntities",
                columns: table => new
                {
                    PeerDIDEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PeerDID = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    TenantEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerDIDEntities", x => x.PeerDIDEntityId);
                    table.ForeignKey(
                        name: "FK_PeerDIDEntities_TenantEntities_TenantEntityId",
                        column: x => x.TenantEntityId,
                        principalTable: "TenantEntities",
                        principalColumn: "TenantEntityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerDIDEntities_TenantEntityId",
                table: "PeerDIDEntities",
                column: "TenantEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuingKeys_TenantEntities_TenantEntityId",
                table: "IssuingKeys",
                column: "TenantEntityId",
                principalTable: "TenantEntities",
                principalColumn: "TenantEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssuingKeys_TenantEntities_TenantEntityId",
                table: "IssuingKeys");

            migrationBuilder.DropTable(
                name: "PeerDIDEntities");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuingKeys_TenantEntities_TenantEntityId",
                table: "IssuingKeys",
                column: "TenantEntityId",
                principalTable: "TenantEntities",
                principalColumn: "TenantEntityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
