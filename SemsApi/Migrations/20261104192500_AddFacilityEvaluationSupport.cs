using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityEvaluationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Facilities",
                columns: table => new
                {
                    FacilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.FacilityId);
                });

            migrationBuilder.CreateTable(
                name: "FacilityEvaluationSubmissions",
                schema: "Eval",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    EvaluationTypeId = table.Column<int>(type: "int", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: true),
                    EvaluationSessionId = table.Column<int>(type: "int", nullable: true),
                    SchoolYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityEvaluationSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_FacilityEvaluationSubmissions_EvaluationSessions_EvaluationSessionId",
                        column: x => x.EvaluationSessionId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationSessions",
                        principalColumn: "EvaluationSessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacilityEvaluationSubmissions_EvaluationType_EvaluationTypeId",
                        column: x => x.EvaluationTypeId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationType",
                        principalColumn: "EvaluationTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacilityEvaluationSubmissions_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "FacilityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacilityEvaluationSubmissions_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Eval",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacilityEvaluationSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacilityEvaluationAnswers",
                schema: "Eval",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    CatNo = table.Column<int>(type: "int", nullable: false),
                    QnNo = table.Column<int>(type: "int", nullable: false),
                    RateVal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityEvaluationAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_FacilityEvaluationAnswers_FacilityEvaluationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "Eval",
                        principalTable: "FacilityEvaluationSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_FacilityName",
                table: "Facilities",
                column: "FacilityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationAnswers_SubmissionId_QnNo",
                schema: "Eval",
                table: "FacilityEvaluationAnswers",
                columns: new[] { "SubmissionId", "QnNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_EvaluationSessionId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                column: "EvaluationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_EvaluationTypeId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                column: "EvaluationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_FacilityId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_FormId",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityEvaluationSubmissions_StudentId_FacilityId_SchoolYear_Term",
                schema: "Eval",
                table: "FacilityEvaluationSubmissions",
                columns: new[] { "StudentId", "FacilityId", "SchoolYear", "Term" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacilityEvaluationAnswers",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "FacilityEvaluationSubmissions",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "Facilities");
        }
    }
}
