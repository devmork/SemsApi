using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidanceEvaluationLogUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuidanceEvaluationLog_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog");

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceEvaluationLog_StudentId_Sy_Term",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                columns: new[] { "StudentId", "Sy", "Term" },
                unique: true,
                filter: "[Sy] IS NOT NULL AND [Term] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuidanceEvaluationLog_StudentId_Sy_Term",
                schema: "Eval",
                table: "GuidanceEvaluationLog");

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceEvaluationLog_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                column: "StudentId");
        }
    }
}
