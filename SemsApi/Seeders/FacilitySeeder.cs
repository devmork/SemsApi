using Microsoft.EntityFrameworkCore;
using SemsApi.Data;
using SemsApi.Models;

namespace SemsApi.Seeders
{
    public static class FacilitySeeder
    {
        private static readonly string[] DefaultFacilities = new[]
        {
            "Library",
            "Canteen",
            "Clinic",
            "Gym",
            "Computer Laboratory",
            "Guidance Office",
        };

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            foreach (var name in DefaultFacilities)
            {
                var exists = await context.Facilities
                    .AnyAsync(f => f.FacilityName.ToLower() == name.ToLower());
                if (exists) continue;

                context.Facilities.Add(new Facility
                {
                    FacilityName = name,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
