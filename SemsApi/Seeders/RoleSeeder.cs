using Microsoft.AspNetCore.Identity;
using SemsApi.Models;

namespace SemsApi.Seeders
{
    public static class RoleSeeder
    {
        // Role definitions kept in one place so they're easy to change later.
        private static readonly Role[] DefaultRoles = new[]
        {
            new Role
            {
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "System Administrator",
                IsActive = true,
                SortOrder = 1
            },
            new Role
            {
                Name = "Teacher",
                NormalizedName = "TEACHER",
                Description = "Classroom Teacher",
                IsActive = true,
                SortOrder = 2
            },
            new Role
            {
                Name = "Student",
                NormalizedName = "STUDENT",
                Description = "Student",
                IsActive = true,
                SortOrder = 3
            },
            new Role
            {
                Name = "QA",
                NormalizedName = "QA",
                Description = "Quality Assurance",
                IsActive = true,
                SortOrder = 4
            }
        };

        /// <summary>
        /// Creates any missing default roles. Call this once during startup
        /// (see Program.cs) before the app starts handling requests.
        /// </summary>
        public static async Task SeedAsync(RoleManager<Role> roleManager)
        {
            foreach (var role in DefaultRoles)
            {
                var exists = await roleManager.RoleExistsAsync(role.Name!);
                if (exists)
                    continue;

                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    // Collect messages so the failure is visible in startup logs.
                    var errors = string.Join("; ",
                        result.Errors.Select(e => $"{e.Code}: {e.Description}"));

                    throw new InvalidOperationException(
                        $"Failed to seed role '{role.Name}'. Errors: {errors}");
                }
            }
        }
    }
}