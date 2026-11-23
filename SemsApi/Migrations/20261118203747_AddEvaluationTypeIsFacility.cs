using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationTypeIsFacility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_SchoolYear_Term",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions");

            migrationBuilder.AddColumn<bool>(
                name: "IsFacility",
                schema: "Eval",
                table: "EvaluationType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_EvaluationSessionId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                columns: new[] { "StudentId", "FacilityId", "EvaluationSessionId" },
                unique: true,
                filter: "[EvaluationSessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_SchoolYear_Term",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                columns: new[] { "StudentId", "FacilityId", "SchoolYear", "Term" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_EvaluationSessionId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_SchoolYear_Term",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions");

            migrationBuilder.DropColumn(
                name: "IsFacility",
                schema: "Eval",
                table: "EvaluationType");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_SchoolYear_Term",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                columns: new[] { "StudentId", "FacilityId", "SchoolYear", "Term" },
                unique: true);
        }
    }
}
