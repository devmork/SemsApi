using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityAndEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Eval");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoogleSubjectId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizedEmailDomains",
                columns: table => new
                {
                    DomainId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedEmailDomains", x => x.DomainId);
                });

            migrationBuilder.CreateTable(
                name: "GuidanceEvaluationLog",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Sy = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true),
                    DateEvaluated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuidanceEvaluationLog", x => x.Recno);
                });

            migrationBuilder.CreateTable(
                name: "GuidanceEvaluationResult",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CatNo = table.Column<int>(type: "int", nullable: true),
                    QnNo = table.Column<int>(type: "int", nullable: true),
                    Sy = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true),
                    RateVal = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuidanceEvaluationResult", x => x.Recno);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationCategory",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CatNo = table.Column<int>(type: "int", nullable: true),
                    CatRn = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CatName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationCategory", x => x.Recno);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationResult",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CatNo = table.Column<int>(type: "int", nullable: true),
                    QnNo = table.Column<int>(type: "int", nullable: true),
                    QnName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationResult", x => x.Recno);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationStrength",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TeacherId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true),
                    CommentName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationStrength", x => x.Recno);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StudentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherId);
                    table.ForeignKey(
                        name: "FK_Teachers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName", "SortOrder" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Administrator", true, "Admin", "ADMIN", 1 },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Classroom Teacher", true, "Teacher", "TEACHER", 2 },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Student", true, "Student", "STUDENT", 3 }
                });

            migrationBuilder.InsertData(
                table: "AuthorizedEmailDomains",
                columns: new[] { "DomainId", "CreatedAt", "Domain", "InstitutionName", "IsActive" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "dmc.edu.ph", "DMC College Foundation, Inc.", true });

            migrationBuilder.InsertData(
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                columns: new[] { "Recno", "CatName", "CatNo", "CatRate", "CatRn", "EvalType" },
                values: new object[,]
                {
                    { 1, "Teaching Effectiveness", 1, 25.00m, "A", "Teacher Evaluation" },
                    { 2, "Classroom Management", 2, 20.00m, "B", "Teacher Evaluation" },
                    { 3, "Communication Skills", 3, 20.00m, "C", "Teacher Evaluation" },
                    { 4, "Subject Knowledge", 4, 20.00m, "D", "Teacher Evaluation" },
                    { 5, "Student Engagement", 5, 15.00m, "E", "Teacher Evaluation" }
                });

            migrationBuilder.InsertData(
                schema: "Eval",
                table: "TeacherEvaluationResult",
                columns: new[] { "Recno", "CatNo", "EvalType", "QnName", "QnNo" },
                values: new object[,]
                {
                    { 1, 1, "Teacher Evaluation", "The teacher explains lessons clearly and in an organized manner.", 1 },
                    { 2, 1, "Teacher Evaluation", "The teacher uses appropriate examples and illustrations.", 2 },
                    { 3, 1, "Teacher Evaluation", "The teacher adjusts teaching methods to different learning styles.", 3 },
                    { 4, 1, "Teacher Evaluation", "The teacher provides timely and constructive feedback on student work.", 4 },
                    { 5, 2, "Teacher Evaluation", "The teacher maintains an orderly and respectful classroom environment.", 1 },
                    { 6, 2, "Teacher Evaluation", "The teacher manages time effectively during class periods.", 2 },
                    { 7, 2, "Teacher Evaluation", "The teacher handles student misbehavior fairly and consistently.", 3 },
                    { 8, 2, "Teacher Evaluation", "Classroom rules and expectations are clearly communicated.", 4 },
                    { 9, 3, "Teacher Evaluation", "The teacher communicates expectations and instructions clearly.", 1 },
                    { 10, 3, "Teacher Evaluation", "The teacher listens to and responds to student questions.", 2 },
                    { 11, 3, "Teacher Evaluation", "The teacher uses language appropriate to the students' level.", 3 },
                    { 12, 3, "Teacher Evaluation", "The teacher provides opportunities for students to express ideas.", 4 },
                    { 13, 4, "Teacher Evaluation", "The teacher demonstrates thorough knowledge of the subject matter.", 1 },
                    { 14, 4, "Teacher Evaluation", "The teacher relates the subject to real-life situations.", 2 },
                    { 15, 4, "Teacher Evaluation", "The teacher stays updated with current developments in the field.", 3 },
                    { 16, 4, "Teacher Evaluation", "The teacher answers content-related questions accurately.", 4 },
                    { 17, 5, "Teacher Evaluation", "The teacher encourages active participation from all students.", 1 },
                    { 18, 5, "Teacher Evaluation", "The teacher creates a motivating and supportive learning atmosphere.", 2 },
                    { 19, 5, "Teacher Evaluation", "The teacher uses varied activities to sustain student interest.", 3 },
                    { 20, 5, "Teacher Evaluation", "The teacher recognizes and praises student effort and achievement.", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GoogleSubjectId",
                table: "AspNetUsers",
                column: "GoogleSubjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedEmailDomains_Domain",
                table: "AuthorizedEmailDomains",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_EmployeeNumber",
                table: "Teachers",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuthorizedEmailDomains");

            migrationBuilder.DropTable(
                name: "GuidanceEvaluationLog",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "GuidanceEvaluationResult",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationCategory",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationResult",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationStrength",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
