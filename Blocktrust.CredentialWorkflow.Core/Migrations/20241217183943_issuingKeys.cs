using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class issuingKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssuingKey",
                columns: table => new
                {
                    IssuingKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    KeyType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PrivateKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TenantEntityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuingKey", x => x.IssuingKeyId);
                    table.ForeignKey(
                        name: "FK_IssuingKey_TenantEntities_TenantEntityId",
                        column: x => x.TenantEntityId,
                        principalTable: "TenantEntities",
                        principalColumn: "TenantEntityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssuingKey_TenantEntityId",
                table: "IssuingKey",
                column: "TenantEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssuingKey");
        }
    }
}
