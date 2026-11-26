using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsFacilityPropertyInFacilityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFacility",
                schema: "Eval",
                table: "EvaluationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFacility",
                schema: "Eval",
                table: "EvaluationType",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
