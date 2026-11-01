using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class FixEvaluationSubmissionIdentityAndSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TeacherEvaluationSubmissions",
                newName: "TeacherEvaluationSubmissions",
                newSchema: "Eval");

            migrationBuilder.RenameTable(
                name: "TeacherEvaluationAnswers",
                newName: "TeacherEvaluationAnswers",
                newSchema: "Eval");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TeacherEvaluationSubmissions",
                schema: "Eval",
                newName: "TeacherEvaluationSubmissions");

            migrationBuilder.RenameTable(
                name: "TeacherEvaluationAnswers",
                schema: "Eval",
                newName: "TeacherEvaluationAnswers");
        }
    }
}
