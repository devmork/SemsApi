using Microsoft.AspNetCore.Identity;
using SemsApi.Models;

namespace SemsApi.Seeders
{
    public static class RoleSeeder
    {
        private static readonly Role[] DefaultRoles = new[]
        {
            new Role
            {
                Name = "Admin",
                NormalizedName = "ADMIN",
                IsActive = true,
                SortOrder = 1
            },
            new Role
            {
                Name = "Qa",
                NormalizedName = "QA",
                IsActive = true,
                SortOrder = 2
            },
            new Role
            {
                Name = "Student",
                NormalizedName = "STUDENT",
                IsActive = true,
                SortOrder = 3
            },
        };

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
                    var errors = string.Join("; ",result.Errors.Select(e => $"{e.Code}: {e.Description}"));

                    throw new InvalidOperationException($"Failed to seed role '{role.Name}'. Errors: {errors}");
                }
            }
        }
    }
}