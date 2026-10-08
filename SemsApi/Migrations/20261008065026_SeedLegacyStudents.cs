using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SemsApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedLegacyStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "GoogleSubjectId", "LastLoginAt", "LastName", "LockoutEnabled", "LockoutEnd", "MiddleName", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Status", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { 1, 0, "92ac3653-5441-40f5-bb9a-cc1ed15f9fb9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "jelah.cuevas@dmc.edu.ph", true, "Jelah", "test-google-sub-001", null, "Cuevas", false, null, null, "JELAH.CUEVAS@DMC.EDU.PH", "JELAH.CUEVAS@DMC.EDU.PH", null, null, false, null, "Active", false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "jelah.cuevas@dmc.edu.ph" },
                    { 2, 0, "861e13a6-b684-49c8-a2ab-25c476f3f0cd", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "shevonn.salasala@dmc.edu.ph", true, "Shevonn", "test-google-sub-002", null, "Salasala", false, null, null, "SHEVONN.SALASALA@DMC.EDU.PH", "SHEVONN.SALASALA@DMC.EDU.PH", null, null, false, null, "Active", false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "shevonn.salasala@dmc.edu.ph" },
                    { 3, 0, "1b5e1ea9-de26-428f-8af8-b1f8020d813c", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "chaser.gone@dmc.edu.ph", true, "Chaser", "test-google-sub-003", null, "Gone", false, null, null, "CHASER.GONE@DMC.EDU.PH", "CHASER.GONE@DMC.EDU.PH", null, null, false, null, "Active", false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "chaser.gone@dmc.edu.ph" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "GradeLevel", "SchoolYear", "Section", "Status", "StudentNumber", "UserId" },
                values: new object[,]
                {
                    { 1, "Grade 6", "2025-2026", "GRACE", "Active", "2020-0331", 1 },
                    { 2, "Grade 7", "2025-2026", "St. Solomon Leclerq", "Active", "2025-2219", 2 },
                    { 3, "Kindergarten", "2025-2026", "KINDERGARTEN", "Active", "2025-2235", 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "StudentId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "StudentId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "StudentId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
