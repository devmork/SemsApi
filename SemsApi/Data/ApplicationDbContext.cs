using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SemsApi.Models;
using SemsApi.Models.Eval;

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

        // Evaluation
        public DbSet<TeacherEvaluationCategory> TeacherEvaluationCategories => Set<TeacherEvaluationCategory>();
        public DbSet<TeacherEvaluationResult> TeacherEvaluationResults => Set<TeacherEvaluationResult>();
        public DbSet<GuidanceEvaluationResult> GuidanceEvaluationResults => Set<GuidanceEvaluationResult>();
        public DbSet<GuidanceEvaluationLog> GuidanceEvaluationLogs => Set<GuidanceEvaluationLog>();
        public DbSet<TeacherEvaluationStrength> TeacherEvaluationStrengths => Set<TeacherEvaluationStrength>();

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

            // ===== Seed Authorized Domain =====
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

            // ===== Seed Evaluation Categories & Questions =====
            modelBuilder.Entity<TeacherEvaluationCategory>().HasData(
                new TeacherEvaluationCategory { Recno = 1, EvalType = "Teacher Evaluation", CatNo = 1, CatRn = "A", CatName = "Teaching Effectiveness", CatRate = 25.00m },
                new TeacherEvaluationCategory { Recno = 2, EvalType = "Teacher Evaluation", CatNo = 2, CatRn = "B", CatName = "Classroom Management", CatRate = 20.00m },
                new TeacherEvaluationCategory { Recno = 3, EvalType = "Teacher Evaluation", CatNo = 3, CatRn = "C", CatName = "Communication Skills", CatRate = 20.00m },
                new TeacherEvaluationCategory { Recno = 4, EvalType = "Teacher Evaluation", CatNo = 4, CatRn = "D", CatName = "Subject Knowledge", CatRate = 20.00m },
                new TeacherEvaluationCategory { Recno = 5, EvalType = "Teacher Evaluation", CatNo = 5, CatRn = "E", CatName = "Student Engagement", CatRate = 15.00m }
            );

            modelBuilder.Entity<TeacherEvaluationResult>().HasData(
                new TeacherEvaluationResult { Recno = 1, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 1, QnName = "The teacher explains lessons clearly and in an organized manner." },
                new TeacherEvaluationResult { Recno = 2, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 2, QnName = "The teacher uses appropriate examples and illustrations." },
                new TeacherEvaluationResult { Recno = 3, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 3, QnName = "The teacher adjusts teaching methods to different learning styles." },
                new TeacherEvaluationResult { Recno = 4, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 4, QnName = "The teacher provides timely and constructive feedback on student work." },

                new TeacherEvaluationResult { Recno = 5, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 1, QnName = "The teacher maintains an orderly and respectful classroom environment." },
                new TeacherEvaluationResult { Recno = 6, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 2, QnName = "The teacher manages time effectively during class periods." },
                new TeacherEvaluationResult { Recno = 7, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 3, QnName = "The teacher handles student misbehavior fairly and consistently." },
                new TeacherEvaluationResult { Recno = 8, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 4, QnName = "Classroom rules and expectations are clearly communicated." },

                new TeacherEvaluationResult { Recno = 9, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 1, QnName = "The teacher communicates expectations and instructions clearly." },
                new TeacherEvaluationResult { Recno = 10, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 2, QnName = "The teacher listens to and responds to student questions." },
                new TeacherEvaluationResult { Recno = 11, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 3, QnName = "The teacher uses language appropriate to the students' level." },
                new TeacherEvaluationResult { Recno = 12, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 4, QnName = "The teacher provides opportunities for students to express ideas." },

                new TeacherEvaluationResult { Recno = 13, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 1, QnName = "The teacher demonstrates thorough knowledge of the subject matter." },
                new TeacherEvaluationResult { Recno = 14, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 2, QnName = "The teacher relates the subject to real-life situations." },
                new TeacherEvaluationResult { Recno = 15, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 3, QnName = "The teacher stays updated with current developments in the field." },
                new TeacherEvaluationResult { Recno = 16, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 4, QnName = "The teacher answers content-related questions accurately." },

                new TeacherEvaluationResult { Recno = 17, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 1, QnName = "The teacher encourages active participation from all students." },
                new TeacherEvaluationResult { Recno = 18, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 2, QnName = "The teacher creates a motivating and supportive learning atmosphere." },
                new TeacherEvaluationResult { Recno = 19, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 3, QnName = "The teacher uses varied activities to sustain student interest." },
                new TeacherEvaluationResult { Recno = 20, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 4, QnName = "The teacher recognizes and praises student effort and achievement." }
            );
        }

    }
}
