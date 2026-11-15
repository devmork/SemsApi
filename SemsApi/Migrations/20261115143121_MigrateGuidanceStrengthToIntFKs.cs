using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class MigrateGuidanceStrengthToIntFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationComment_StudentId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationComment_TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceEvaluationAnswer_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceEvaluationLog_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuidanceEvaluationLog_Students_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuidanceEvaluationAnswer_Students_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationComment_Students_StudentId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationComment_Teachers_TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuidanceEvaluationLog_Students_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog");

            migrationBuilder.DropForeignKey(
                name: "FK_GuidanceEvaluationAnswer_Students_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationComment_Students_StudentId",
                schema: "Eval",
                table: "TeacherEvaluationComment");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationComment_Teachers_TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationComment");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluationComment_StudentId",
                schema: "Eval",
                table: "TeacherEvaluationComment");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluationComment_TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationComment");

            migrationBuilder.DropIndex(
                name: "IX_GuidanceEvaluationAnswer_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer");

            migrationBuilder.DropIndex(
                name: "IX_GuidanceEvaluationLog_StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                schema: "Eval",
                table: "TeacherEvaluationComment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationAnswer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                schema: "Eval",
                table: "GuidanceEvaluationLog",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
