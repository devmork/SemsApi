using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluationSessions",
                schema: "Eval",
                columns: table => new
                {
                    EvaluationSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormId = table.Column<int>(type: "int", nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    OpensAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosesAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationSessions", x => x.EvaluationSessionId);
                    table.ForeignKey(
                        name: "FK_EvaluationSessions_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationSessions_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Eval",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "EvaluationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_CreatedBy",
                schema: "Eval",
                table: "EvaluationSessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_FormId_SchoolYear_Term",
                schema: "Eval",
                table: "EvaluationSessions",
                columns: new[] { "FormId", "SchoolYear", "Term" },
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSessions_Status_OpensAtUtc_ClosesAtUtc",
                schema: "Eval",
                table: "EvaluationSessions",
                columns: new[] { "Status", "OpensAtUtc", "ClosesAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationSubmissions_EvaluationSessions_EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "EvaluationSessionId",
                principalSchema: "Eval",
                principalTable: "EvaluationSessions",
                principalColumn: "EvaluationSessionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationSubmissions_EvaluationSessions_EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions");

            migrationBuilder.DropTable(
                name: "EvaluationSessions",
                schema: "Eval");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluationSubmissions_EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions");

            migrationBuilder.DropColumn(
                name: "EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions");
        }
    }
}
