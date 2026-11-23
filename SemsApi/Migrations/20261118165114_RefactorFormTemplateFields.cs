using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFormTemplateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EvaluationForms_Code",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationForms_ProgramId",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.DropColumn(
                name: "TargetType",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.RenameColumn(
                name: "GradeBand",
                schema: "Eval",
                table: "EvaluationForms",
                newName: "EducationalStage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EducationalStage",
                schema: "Eval",
                table: "EvaluationForms",
                newName: "GradeBand");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Eval",
                table: "EvaluationForms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                schema: "Eval",
                table: "EvaluationForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetType",
                schema: "Eval",
                table: "EvaluationForms",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationForms_Code",
                schema: "Eval",
                table: "EvaluationForms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationForms_ProgramId",
                schema: "Eval",
                table: "EvaluationForms",
                column: "ProgramId");
        }
    }
}
