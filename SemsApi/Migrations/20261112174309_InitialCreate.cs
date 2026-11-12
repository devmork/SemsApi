using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    PictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                name: "EvaluationType",
                schema: "Eval",
                columns: table => new
                {
                    EvaluationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationType", x => x.EvaluationTypeId);
                });

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
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
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

            migrationBuilder.CreateTable(
                name: "Forms",
                schema: "Eval",
                columns: table => new
                {
                    FormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GradeBand = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TargetType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EvaluationTypeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_Forms_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Forms_EvaluationType_EvaluationTypeId",
                        column: x => x.EvaluationTypeId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationType",
                        principalColumn: "EvaluationTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationCategory",
                schema: "Eval",
                columns: table => new
                {
                    CatNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationTypeId = table.Column<int>(type: "int", nullable: false),
                    CatRn = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CatName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationCategory", x => x.CatNo);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationCategory_EvaluationType_EvaluationTypeId",
                        column: x => x.EvaluationTypeId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationType",
                        principalColumn: "EvaluationTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationStrength",
                schema: "Eval",
                columns: table => new
                {
                    Recno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationTypeId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true),
                    CommentName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationStrength", x => x.Recno);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationStrength_EvaluationType_EvaluationTypeId",
                        column: x => x.EvaluationTypeId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationType",
                        principalColumn: "EvaluationTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationResult",
                schema: "Eval",
                columns: table => new
                {
                    QnNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatNo = table.Column<int>(type: "int", nullable: false),
                    QnName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationResult", x => x.QnNo);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationResult_TeacherEvaluationCategory_CatNo",
                        column: x => x.CatNo,
                        principalSchema: "Eval",
                        principalTable: "TeacherEvaluationCategory",
                        principalColumn: "CatNo",
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
                name: "TeacherEvaluationSubmissions",
                schema: "Eval",
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
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluationSessionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationSubmissions_EvaluationSessions_EvaluationSessionId",
                        column: x => x.EvaluationSessionId,
                        principalSchema: "Eval",
                        principalTable: "EvaluationSessions",
                        principalColumn: "EvaluationSessionId",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationAnswers",
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
                    table.PrimaryKey("PK_TeacherEvaluationAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationAnswers_TeacherEvaluationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "Eval",
                        principalTable: "TeacherEvaluationSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationType_EvaluationTypeName",
                schema: "Eval",
                table: "EvaluationType",
                column: "EvaluationTypeName",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Forms_Code",
                schema: "Eval",
                table: "Forms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_CreatedBy",
                schema: "Eval",
                table: "Forms",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_EvaluationTypeId",
                schema: "Eval",
                table: "Forms",
                column: "EvaluationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_ProgramId",
                schema: "Eval",
                table: "Forms",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

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
                name: "IX_StudentSubjectEnrollments_StudentId_SubjectOfferingId",
                table: "StudentSubjectEnrollments",
                columns: new[] { "StudentId", "SubjectOfferingId" },
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
                name: "IX_SubjectOfferings_EdpCode_Section_SchoolYear_Term",
                table: "SubjectOfferings",
                columns: new[] { "EdpCode", "Section", "SchoolYear", "Term" },
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
                schema: "Eval",
                table: "TeacherEvaluationAnswers",
                columns: new[] { "SubmissionId", "QnNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationCategory_EvaluationTypeId",
                schema: "Eval",
                table: "TeacherEvaluationCategory",
                column: "EvaluationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationResult_CatNo",
                schema: "Eval",
                table: "TeacherEvaluationResult",
                column: "CatNo");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationStrength_EvaluationTypeId",
                schema: "Eval",
                table: "TeacherEvaluationStrength",
                column: "EvaluationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_EvaluationSessionId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "EvaluationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_EvaluationTypeId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "EvaluationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_FormId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_StudentId_SchoolYear_Term",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                columns: new[] { "StudentId", "SchoolYear", "Term" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_StudentId_SubjectOfferingId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                columns: new[] { "StudentId", "SubjectOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_SubjectOfferingId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationSubmissions_TeacherId",
                schema: "Eval",
                table: "TeacherEvaluationSubmissions",
                column: "TeacherId");

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
                name: "FacilityEvaluationAnswers",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "GuidanceEvaluationLog",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "GuidanceEvaluationResult",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "StudentSubjectEnrollments");

            migrationBuilder.DropTable(
                name: "StudentTermEnrollments");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationAnswers",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationResult",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationStrength",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "FacilityEvaluationSubmissions",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationSubmissions",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationCategory",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "Facilities");

            migrationBuilder.DropTable(
                name: "EvaluationSessions",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "SubjectOfferings");

            migrationBuilder.DropTable(
                name: "Forms",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "EvaluationType",
                schema: "Eval");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
