using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryQuestionProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationQuestion_EvaluationCategory_CatNo",
                schema: "Eval",
                table: "EvaluationQuestion");

            migrationBuilder.DropColumn(
                name: "CatRn",
                schema: "Eval",
                table: "EvaluationCategory");

            migrationBuilder.RenameColumn(
                name: "QnNo",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "CatNo",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherEvaluationAnswers_SubmissionId_QnNo",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                newName: "IX_TeacherEvaluationAnswers_SubmissionId_QuestionId");

            migrationBuilder.RenameColumn(
                name: "QnNo",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "CatNo",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_FacilityEvaluationAnswers_SubmissionId_QnNo",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                newName: "IX_FacilityEvaluationAnswers_SubmissionId_QuestionId");

            migrationBuilder.RenameColumn(
                name: "QnName",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "QuestionName");

            migrationBuilder.RenameColumn(
                name: "CatNo",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "QnNo",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationQuestion_CatNo",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "IX_EvaluationQuestion_CategoryId");

            migrationBuilder.RenameColumn(
                name: "CatRate",
                schema: "Eval",
                table: "EvaluationCategory",
                newName: "CategoryRate");

            migrationBuilder.RenameColumn(
                name: "CatName",
                schema: "Eval",
                table: "EvaluationCategory",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "CatNo",
                schema: "Eval",
                table: "EvaluationCategory",
                newName: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationQuestion_EvaluationCategory_CategoryId",
                schema: "Eval",
                table: "EvaluationQuestion",
                column: "CategoryId",
                principalSchema: "Eval",
                principalTable: "EvaluationCategory",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationQuestion_EvaluationCategory_CategoryId",
                schema: "Eval",
                table: "EvaluationQuestion");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                newName: "QnNo");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                newName: "CatNo");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherEvaluationAnswers_SubmissionId_QuestionId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                newName: "IX_TeacherEvaluationAnswers_SubmissionId_QnNo");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                newName: "QnNo");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                newName: "CatNo");

            migrationBuilder.RenameIndex(
                name: "IX_FacilityEvaluationAnswers_SubmissionId_QuestionId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                newName: "IX_FacilityEvaluationAnswers_SubmissionId_QnNo");

            migrationBuilder.RenameColumn(
                name: "QuestionName",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "QnName");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "CatNo");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "QnNo");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationQuestion_CategoryId",
                schema: "Eval",
                table: "EvaluationQuestion",
                newName: "IX_EvaluationQuestion_CatNo");

            migrationBuilder.RenameColumn(
                name: "CategoryRate",
                schema: "Eval",
                table: "EvaluationCategory",
                newName: "CatRate");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                schema: "Eval",
                table: "EvaluationCategory",
                newName: "CatName");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "Eval",
                table: "EvaluationCategory",
                newName: "CatNo");

            migrationBuilder.AddColumn<string>(
                name: "CatRn",
                schema: "Eval",
                table: "EvaluationCategory",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationQuestion_EvaluationCategory_CatNo",
                schema: "Eval",
                table: "EvaluationQuestion",
                column: "CatNo",
                principalTable: "EvaluationCategory",
                principalSchema: "Eval",
                principalColumn: "CatNo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
