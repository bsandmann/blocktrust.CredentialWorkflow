using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class workflowOutcomes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutcomeEntities");

            migrationBuilder.CreateTable(
                name: "WorkflowOutcomeEntities",
                columns: table => new
                {
                    WorkflowOutcomeEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowOutcomeState = table.Column<string>(type: "text", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorJson = table.Column<string>(type: "text", nullable: true),
                    OutcomeJson = table.Column<string>(type: "text", nullable: true),
                    ExecutionContext = table.Column<string>(type: "text", nullable: true),
                    WorkflowEntityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowOutcomeEntities", x => x.WorkflowOutcomeEntityId);
                    table.ForeignKey(
                        name: "FK_WorkflowOutcomeEntities_WorkflowEntities_WorkflowEntityId",
                        column: x => x.WorkflowEntityId,
                        principalTable: "WorkflowEntities",
                        principalColumn: "WorkflowEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowOutcomeEntities_WorkflowEntityId",
                table: "WorkflowOutcomeEntities",
                column: "WorkflowEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowOutcomeEntities");

            migrationBuilder.CreateTable(
                name: "OutcomeEntities",
                columns: table => new
                {
                    OutcomeEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorJson = table.Column<string>(type: "text", nullable: true),
                    ExecutionContext = table.Column<string>(type: "text", nullable: true),
                    OutcomeJson = table.Column<string>(type: "text", nullable: true),
                    OutcomeState = table.Column<string>(type: "text", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
        }
    }
}
