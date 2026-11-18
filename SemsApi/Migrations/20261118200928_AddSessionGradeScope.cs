using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionGradeScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.AddColumn<int>(
                name: "GradeLevelFrom",
                schema: "Eval",
                table: "EvaluationSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term_GradeLevelFrom_GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions",
                columns: new[] { "FormId", "SchoolYear", "Term", "GradeLevelFrom", "GradeLevelTo" },
                unique: true,
                filter: "[Status] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term_GradeLevelFrom_GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropColumn(
                name: "GradeLevelFrom",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropColumn(
                name: "GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term",
                schema: "Eval",
                table: "EvaluationSessions",
                columns: new[] { "FormId", "SchoolYear", "Term" },
                unique: true,
                filter: "[Status] <> 3");
        }
    }
}
