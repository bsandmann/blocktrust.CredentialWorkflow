using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class renamings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutcomeJson",
                table: "WorkflowOutcomeEntities",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "ErrorJson",
                table: "WorkflowOutcomeEntities",
                newName: "ActionOutcomesJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "WorkflowOutcomeEntities",
                newName: "OutcomeJson");

            migrationBuilder.RenameColumn(
                name: "ActionOutcomesJson",
                table: "WorkflowOutcomeEntities",
                newName: "ErrorJson");
        }
    }
}
