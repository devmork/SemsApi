using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFormScopedQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationQuestion",
                schema: "Eval");

            migrationBuilder.CreateTable(
                name: "EvaluationFormCategory",
                schema: "Eval",
                columns: table => new
                {
                    FormCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFormCategory", x => x.FormCategoryId);
                    table.ForeignKey(
                        name: "FK_EvaluationFormCategory_EvaluationCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationCategory",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationFormCategory_EvaluationForms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationForms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFormQuestion",
                schema: "Eval",
                columns: table => new
                {
                    FormQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormCategoryId = table.Column<int>(type: "int", nullable: false),
                    QuestionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFormQuestion", x => x.FormQuestionId);
                    table.ForeignKey(
                        name: "FK_EvaluationFormQuestion_EvaluationFormCategory_FormCategoryId",
                        column: x => x.FormCategoryId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationFormCategory",
                        principalColumn: "FormCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationFormQuestionAnswer",
                schema: "Eval",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    FormQuestionId = table.Column<int>(type: "int", nullable: false),
                    RateVal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationFormQuestionAnswer", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_EvaluationFormQuestionAnswer_EvaluationFormQuestion_FormQuestionId",
                        column: x => x.FormQuestionId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationFormQuestion",
                        principalColumn: "FormQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationAnswers_QuestionId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationAnswers_QuestionId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFormCategory_CategoryId",
                schema: "Eval",
                table: "EvaluationFormCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFormCategory_FormId_CategoryId",
                schema: "Eval",
                table: "EvaluationFormCategory",
                columns: new[] { "FormId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFormQuestion_FormCategoryId_DisplayOrder",
                schema: "Eval",
                table: "EvaluationFormQuestion",
                columns: new[] { "FormCategoryId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFormQuestionAnswer_FormQuestionId",
                schema: "Eval",
                table: "EvaluationFormQuestionAnswer",
                column: "FormQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationFormQuestionAnswer_SubmissionId_FormQuestionId",
                schema: "Eval",
                table: "EvaluationFormQuestionAnswer",
                columns: new[] { "SubmissionId", "FormQuestionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FacilityEvaluationAnswers_EvaluationFormQuestion_QuestionId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                column: "QuestionId",
                principalSchema: "Eval",
                principalTable: "EvaluationFormQuestion",
                principalColumn: "FormQuestionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluationAnswers_EvaluationFormQuestion_QuestionId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                column: "QuestionId",
                principalSchema: "Eval",
                principalTable: "EvaluationFormQuestion",
                principalColumn: "FormQuestionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacilityEvaluationAnswers_EvaluationFormQuestion_QuestionId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluationAnswers_EvaluationFormQuestion_QuestionId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers");

            migrationBuilder.DropTable(
                name: "EvaluationFormQuestionAnswer",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "EvaluationFormQuestion",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "EvaluationFormCategory",
                schema: "Eval");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluationAnswers_QuestionId",
                schema: "Eval",
                table: "TeacherEvaluationAnswers");

            migrationBuilder.DropIndex(
                name: "IX_FacilityEvaluationAnswers_QuestionId",
                schema: "Eval",
                table: "FacilityEvaluationAnswers");

            migrationBuilder.CreateTable(
                name: "EvaluationQuestion",
                schema: "Eval",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    QuestionName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationQuestion", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_EvaluationQuestion_EvaluationCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationCategory",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationQuestion_CategoryId",
                schema: "Eval",
                table: "EvaluationQuestion",
                column: "CategoryId");
        }
    }
}
