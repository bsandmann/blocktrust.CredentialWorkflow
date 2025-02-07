using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class PeerDIDSecretes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeerDIDSecrets",
                columns: table => new
                {
                    PeerDIDSecretId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kid = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VerificationMethodType = table.Column<int>(type: "integer", nullable: false),
                    VerificationMaterialFormat = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerDIDSecrets", x => x.PeerDIDSecretId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerDIDSecrets");
        }
    }
}
