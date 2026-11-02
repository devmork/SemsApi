using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectAndTeacherEvaluationRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "StudentTermEnrollments",
                columns: table => new
                {
                    StudentTermEnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GradeLevel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnrolledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTermEnrollments", x => x.StudentTermEnrollmentId);
                    table.ForeignKey(
                        name: "FK_StudentTermEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "SubjectOfferings",
                columns: table => new
                {
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EdpCode = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GradeLevel = table.Column<int>(type: "int", nullable: false),
                    Section = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: true),
                    AdviserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectOfferings", x => x.SubjectOfferingId);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_Teachers_AdviserId",
                        column: x => x.AdviserId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentSubjectEnrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnrolledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DroppedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSubjectEnrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_StudentSubjectEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSubjectEnrollments_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "SubjectOfferingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    EvaluationTypeId = table.Column<int>(type: "int", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: true),
                    SchoolYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationSubmissions_EvaluationType_EvaluationTypeId",
                        column: x => x.EvaluationTypeId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationType",
                        principalColumn: "EvaluationTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationSubmissions_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Eval",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationSubmissions_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "SubjectOfferingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationSubmissions_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationAnswers",
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
                    table.PrimaryKey("PK_TeacherEvaluationAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationAnswers_TeacherEvaluationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "TeacherEvaluationSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_EdpCode_Section_SchoolYear_Term",
                table: "SubjectOfferings",
                columns: new[] { "EdpCode", "Section", "SchoolYear", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjectEnrollments_SubjectOfferingId",
                table: "StudentSubjectEnrollments",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTermEnrollments_StudentId_SchoolYear_Term",
                table: "StudentTermEnrollments",
                columns: new[] { "StudentId", "SchoolYear", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_AdviserId",
                table: "SubjectOfferings",
                column: "AdviserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_EdpCode_SchoolYear_Term",
                table: "SubjectOfferings",
                columns: new[] { "EdpCode", "SchoolYear", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SchoolYear_Term",
                table: "SubjectOfferings",
                columns: new[] { "SchoolYear", "Term" });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectId",
                table: "SubjectOfferings",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_TeacherId",
                table: "SubjectOfferings",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationAnswers_SubmissionId_QnNo",
                table: "TeacherEvaluationAnswers",
                columns: new[] { "SubmissionId", "QnNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_EvaluationTypeId",
                table: "TeacherEvaluationSubmissions",
                column: "EvaluationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_FormId",
                table: "TeacherEvaluationSubmissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_StudentId_SchoolYear_Term",
                table: "TeacherEvaluationSubmissions",
                columns: new[] { "StudentId", "SchoolYear", "Term" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_StudentId_SubjectOfferingId",
                table: "TeacherEvaluationSubmissions",
                columns: new[] { "StudentId", "SubjectOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_SubjectOfferingId",
                table: "TeacherEvaluationSubmissions",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_TeacherId",
                table: "TeacherEvaluationSubmissions",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentSubjectEnrollments");

            migrationBuilder.DropTable(
                name: "StudentTermEnrollments");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationAnswers");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationSubmissions");

            migrationBuilder.DropTable(
                name: "SubjectOfferings");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
