using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityToEvaluationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term_GradeLevelFrom_GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.AddColumn<int>(
                name: "FacilityId",
                schema: "Eval",
                table: "EvaluationSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_FacilityId",
                schema: "Eval",
                table: "EvaluationSessions",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_FormId_FacilityId_SchoolYear_Term_GradeLevelFrom_GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions",
                columns: new[] { "FormId", "FacilityId", "SchoolYear", "Term", "GradeLevelFrom", "GradeLevelTo" },
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationSessions_Facilities_FacilityId",
                schema: "Eval",
                table: "EvaluationSessions",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "FacilityId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationSessions_Facilities_FacilityId",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationSessions_FacilityId",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationSessions_FormId_FacilityId_SchoolYear_Term_GradeLevelFrom_GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term_GradeLevelFrom_GradeLevelTo",
                schema: "Eval",
                table: "EvaluationSessions",
                columns: new[] { "FormId", "SchoolYear", "Term", "GradeLevelFrom", "GradeLevelTo" },
                unique: true,
                filter: "[Status] <> 3");
        }
    }
}
