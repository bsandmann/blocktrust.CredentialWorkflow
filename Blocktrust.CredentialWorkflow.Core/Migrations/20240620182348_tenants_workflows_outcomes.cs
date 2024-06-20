using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class tenants_workflows_outcomes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowEntities",
                columns: table => new
                {
                    WorkflowEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkflowState = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "text", unicode: false, nullable: true),
                    TenantEntityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowEntities", x => x.WorkflowEntityId);
                    table.ForeignKey(
                        name: "FK_WorkflowEntities_TenantEntities_TenantEntityId",
                        column: x => x.TenantEntityId,
                        principalTable: "TenantEntities",
                        principalColumn: "TenantEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutcomeEntities",
                columns: table => new
                {
                    OutcomeEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutcomeState = table.Column<int>(type: "integer", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorJson = table.Column<string>(type: "text", nullable: true),
                    OutcomeJson = table.Column<string>(type: "text", nullable: true),
                    WorkflowEntityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutcomeEntities", x => x.OutcomeEntityId);
                    table.ForeignKey(
                        name: "FK_OutcomeEntities_WorkflowEntities_WorkflowEntityId",
                        column: x => x.WorkflowEntityId,
                        principalTable: "WorkflowEntities",
                        principalColumn: "WorkflowEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutcomeEntities_WorkflowEntityId",
                table: "OutcomeEntities",
                column: "WorkflowEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowEntities_TenantEntityId",
                table: "WorkflowEntities",
                column: "TenantEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutcomeEntities");

            migrationBuilder.DropTable(
                name: "WorkflowEntities");
        }
    }
}
