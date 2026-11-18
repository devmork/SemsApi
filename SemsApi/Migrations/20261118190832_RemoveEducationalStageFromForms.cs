using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEducationalStageFromForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationalStage",
                schema: "Eval",
                table: "EvaluationForms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EducationalStage",
                schema: "Eval",
                table: "EvaluationForms",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }
    }
}
