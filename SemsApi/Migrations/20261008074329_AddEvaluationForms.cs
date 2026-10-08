using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluationForm",
                schema: "Eval",
                columns: table => new
                {
                    FormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GradeBand = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationForm", x => x.FormId);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "8302cc37-d793-4fe5-8f52-b9545ef89a7e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "2a7d6b7e-f4a2-460d-a95e-ffd9bef8c7a0");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "7c89bc0e-838d-4c2d-9611-0ce8f955b237");

            migrationBuilder.InsertData(
                schema: "Eval",
                table: "EvaluationForm",
                columns: new[] { "FormId", "Code", "CreatedAt", "GradeBand", "IsActive", "Name", "TargetType" },
                values: new object[,]
                {
                    { 1, "PRE-G3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Preschool-G3", true, "Preschool - Grade 3 Teacher Evaluation", "SubjectTeacher" },
                    { 2, "G4-G6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "G4-G6", true, "Grades 4 - 6 Teacher Evaluation", "SubjectTeacher" },
                    { 3, "JHS-SHS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "JHS-SHS", true, "Junior High & Senior High Teacher Evaluation", "SubjectTeacher" }
                });

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 1,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 2,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 3,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 4,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 5,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 1,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 2,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 3,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 4,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 5,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 6,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 7,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 8,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 9,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 10,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 11,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 12,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 13,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 14,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 15,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 16,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 17,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 18,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 19,
                column: "FormId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 20,
                column: "FormId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationResult_FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationCategory_FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationForm_Code",
                schema: "Eval",
                table: "EvaluationForm",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationCategory_EvaluationForm_FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "EvaluationForm",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationResult_EvaluationForm_FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "EvaluationForm",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationCategory_EvaluationForm_FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationResult_EvaluationForm_FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult");

            migrationBuilder.DropTable(
                name: "EvaluationForm",
                schema: "Eval");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluationResult_FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluationCategory_FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory");

            migrationBuilder.DropColumn(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult");

            migrationBuilder.DropColumn(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "92ac3653-5441-40f5-bb9a-cc1ed15f9fb9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "861e13a6-b684-49c8-a2ab-25c476f3f0cd");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "1b5e1ea9-de26-428f-8af8-b1f8020d813c");
        }
    }
}
