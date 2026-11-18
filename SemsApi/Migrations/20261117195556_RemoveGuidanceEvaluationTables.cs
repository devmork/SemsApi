using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGuidanceEvaluationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuidanceEvaluationAnswer",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "GuidanceEvaluationLog",
                schema: "Eval");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Eval");

            migrationBuilder.CreateTable(
                name: "GuidanceEvaluationAnswer",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CatNo = table.Column<int>(type: "int", nullable: true),
                    QnNo = table.Column<int>(type: "int", nullable: true),
                    RateVal = table.Column<int>(type: "int", nullable: true),
                    Sy = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuidanceEvaluationAnswer", x => x.Recno);
                    table.ForeignKey(
                        name: "FK_GuidanceEvaluationAnswer_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuidanceEvaluationLog",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    DateEvaluated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sy = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuidanceEvaluationLog", x => x.Recno);
                    table.ForeignKey(
                        name: "FK_GuidanceEvaluationLog_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceEvaluationAnswer_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceEvaluationLog_StudentId_Sy_Term",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                columns: new[] { "StudentId", "Sy", "Term" },
                unique: true,
                filter: "[Sy] IS NOT NULL AND [Term] IS NOT NULL");
        }
    }
}
