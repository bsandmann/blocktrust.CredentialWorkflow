using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blocktrust.CredentialWorkflow.Core.Migrations
{
    /// <inheritdoc />
    public partial class enumToStringConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WorkflowState",
                table: "WorkflowEntities",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "OutcomeState",
                table: "OutcomeEntities",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WorkflowState",
                table: "WorkflowEntities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "OutcomeState",
                table: "OutcomeEntities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
