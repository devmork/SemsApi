using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameEvaluationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationSessions_Forms_FormId",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FacilityEvaluationSubmissions_Forms_FormId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Forms_AspNetUsers_CreatedBy",
                schema: "Eval",
                table: "Forms");

            migrationBuilder.DropForeignKey(
                name: "FK_Forms_EvaluationType_EvaluationTypeId",
                schema: "Eval",
                table: "Forms");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationSubmissions_Forms_FormId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Forms",
                schema: "Eval",
                table: "Forms");

            migrationBuilder.RenameTable(
                name: "Forms",
                schema: "Eval",
                newName: "EvaluationForms",
                newSchema: "Eval");

            migrationBuilder.RenameIndex(
                name: "IX_Forms_ProgramId",
                schema: "Eval",
                table: "EvaluationForms",
                newName: "IX_EvaluationForms_ProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_Forms_EvaluationTypeId",
                schema: "Eval",
                table: "EvaluationForms",
                newName: "IX_EvaluationForms_EvaluationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Forms_CreatedBy",
                schema: "Eval",
                table: "EvaluationForms",
                newName: "IX_EvaluationForms_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Forms_Code",
                schema: "Eval",
                table: "EvaluationForms",
                newName: "IX_EvaluationForms_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EvaluationForms",
                schema: "Eval",
                table: "EvaluationForms",
                column: "FormId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationForms_AspNetUsers_CreatedBy",
                schema: "Eval",
                table: "EvaluationForms",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationForms_EvaluationType_EvaluationTypeId",
                schema: "Eval",
                table: "EvaluationForms",
                column: "EvaluationTypeId",
                principalSchema: "Eval",
                principalTable: "EvaluationType",
                principalColumn: "EvaluationTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationSessions_EvaluationForms_FormId",
                schema: "Eval",
                table: "EvaluationSessions",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "EvaluationForms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FacilityEvaluationSubmissions_EvaluationForms_FormId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "EvaluationForms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationSubmissions_EvaluationForms_FormId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "EvaluationForms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationForms_AspNetUsers_CreatedBy",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationForms_EvaluationType_EvaluationTypeId",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationSessions_EvaluationForms_FormId",
                schema: "Eval",
                table: "EvaluationSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_FacilityEvaluationSubmissions_EvaluationForms_FormId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationSubmissions_EvaluationForms_FormId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EvaluationForms",
                schema: "Eval",
                table: "EvaluationForms");

            migrationBuilder.RenameTable(
                name: "EvaluationForms",
                schema: "Eval",
                newName: "Forms",
                newSchema: "Eval");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationForms_ProgramId",
                schema: "Eval",
                table: "Forms",
                newName: "IX_Forms_ProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationForms_EvaluationTypeId",
                schema: "Eval",
                table: "Forms",
                newName: "IX_Forms_EvaluationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationForms_CreatedBy",
                schema: "Eval",
                table: "Forms",
                newName: "IX_Forms_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationForms_Code",
                schema: "Eval",
                table: "Forms",
                newName: "IX_Forms_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Forms",
                schema: "Eval",
                table: "Forms",
                column: "FormId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationSessions_Forms_FormId",
                schema: "Eval",
                table: "EvaluationSessions",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "Forms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FacilityEvaluationSubmissions_Forms_FormId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "Forms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Forms_AspNetUsers_CreatedBy",
                schema: "Eval",
                table: "Forms",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Forms_EvaluationType_EvaluationTypeId",
                schema: "Eval",
                table: "Forms",
                column: "EvaluationTypeId",
                principalSchema: "Eval",
                principalTable: "EvaluationType",
                principalColumn: "EvaluationTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationSubmissions_Forms_FormId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "FormId",
                principalSchema: "Eval",
                principalTable: "Forms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
