using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeFormIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "3620af79-33cf-4dfb-ac87-0275584a49a0");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "5962d107-c7f6-4440-94d7-9beff54b7d21");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "51bdfc13-78d9-4d4f-b08e-044399e716f1");

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 1,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 2,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 3,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 4,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                keyColumn: "Recno",
                keyValue: 5,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 1,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 2,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 3,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 4,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 5,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 6,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 7,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 8,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 9,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 10,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 11,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 12,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 13,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 14,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 15,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 16,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 17,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 18,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 19,
                column: "FormId",
                value: 0);

            migrationBuilder.UpdateData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                keyColumn: "Recno",
                keyValue: 20,
                column: "FormId",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationResult",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FormId",
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
        }
    }
}
