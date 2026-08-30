using Microsoft.EntityFrameworkCore;
using SemsApi.Models;

namespace SemsApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<AuthorizedEmailDomain> AuthorizedEmailDomains => Set<AuthorizedEmailDomain>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---- Role ----
            modelBuilder.Entity<Role>(e =>
            {
                e.HasKey(r => r.RoleId);
                e.Property(r => r.Name).IsRequired().HasMaxLength(50);
                e.HasIndex(r => r.Name).IsUnique();
            });

            // ---- User ----
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.UserId);

                e.Property(u => u.GoogleSubjectId).IsRequired().HasMaxLength(255);
                e.HasIndex(u => u.GoogleSubjectId).IsUnique();

                e.Property(u => u.Email).IsRequired().HasMaxLength(256);
                e.HasIndex(u => u.Email).IsUnique();

                e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                e.Property(u => u.MiddleName).HasMaxLength(100);
                e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                e.Property(u => u.Status).IsRequired().HasMaxLength(20);

                // One-to-many: Role -> User (Restrict: don't let a Role delete cascade into Users)
                e.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Student (optional 1:1 with User) ----
            modelBuilder.Entity<Student>(e =>
            {
                e.HasKey(s => s.StudentId);

                e.Property(s => s.StudentNumber).IsRequired().HasMaxLength(30);
                e.HasIndex(s => s.StudentNumber).IsUnique();

                e.Property(s => s.GradeLevel).IsRequired().HasMaxLength(20);
                e.Property(s => s.Section).IsRequired().HasMaxLength(50);
                e.Property(s => s.SchoolYear).IsRequired().HasMaxLength(20);
                e.Property(s => s.Status).IsRequired().HasMaxLength(20);

                // Unique FK => optional one-to-one
                e.HasIndex(s => s.UserId).IsUnique();

                e.HasOne(s => s.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Teacher (optional 1:1 with User) ----
            modelBuilder.Entity<Teacher>(e =>
            {
                e.HasKey(t => t.TeacherId);

                e.Property(t => t.EmployeeNumber).IsRequired().HasMaxLength(30);
                e.HasIndex(t => t.EmployeeNumber).IsUnique();

                e.Property(t => t.Department).IsRequired().HasMaxLength(100);
                e.Property(t => t.Status).IsRequired().HasMaxLength(20);

                e.HasIndex(t => t.UserId).IsUnique();

                e.HasOne(t => t.User)
                    .WithOne(u => u.Teacher)
                    .HasForeignKey<Teacher>(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- AuthorizedEmailDomain ----
            modelBuilder.Entity<AuthorizedEmailDomain>(e =>
            {
                e.HasKey(d => d.DomainId);
                e.Property(d => d.Domain).IsRequired().HasMaxLength(100);
                e.HasIndex(d => d.Domain).IsUnique();
                e.Property(d => d.InstitutionName).IsRequired().HasMaxLength(150);
            });

            // ---- Seed data (Roles + sample domain) ----
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Admin" },
                new Role { RoleId = 2, Name = "Teacher" },
                new Role { RoleId = 3, Name = "Student" }
            );

            modelBuilder.Entity<AuthorizedEmailDomain>().HasData(
                new AuthorizedEmailDomain
                {
                    DomainId = 1,
                    Domain = "dmc.edu.ph",
                    InstitutionName = "DMC College Foundation, Inc.",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );
        }

    }
}
