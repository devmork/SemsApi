using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SemsApi.Models;
using SemsApi.Models.Eval;

namespace SemsApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Business tables
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<AuthorizedEmailDomain> AuthorizedEmailDomains => Set<AuthorizedEmailDomain>();

        // Evaluation tables
        public DbSet<EvaluationForm> EvaluationForms => Set<EvaluationForm>();
        public DbSet<TeacherEvaluationCategory> TeacherEvaluationCategories => Set<TeacherEvaluationCategory>();
        public DbSet<TeacherEvaluationResult> TeacherEvaluationResults => Set<TeacherEvaluationResult>();
        public DbSet<GuidanceEvaluationResult> GuidanceEvaluationResults => Set<GuidanceEvaluationResult>();
        public DbSet<GuidanceEvaluationLog> GuidanceEvaluationLogs => Set<GuidanceEvaluationLog>();
        public DbSet<TeacherEvaluationStrength> TeacherEvaluationStrengths => Set<TeacherEvaluationStrength>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important! Creates Identity tables

            // =====================================================
            // USER (IdentityUser)
            // =====================================================
            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.GoogleSubjectId)
                    .IsRequired()
                    .HasMaxLength(255);

                e.HasIndex(u => u.GoogleSubjectId)
                    .IsUnique();

                e.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(u => u.MiddleName)
                    .HasMaxLength(100);

                e.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(u => u.Status)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            // =====================================================
            // ROLE (IdentityRole)
            // =====================================================
            modelBuilder.Entity<Role>(e =>
            {
                e.Property(r => r.Description)
                    .HasMaxLength(250);

                e.Property(r => r.IsActive)
                    .HasDefaultValue(true);

                e.Property(r => r.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // =====================================================
            // STUDENT
            // =====================================================
            modelBuilder.Entity<Student>(e =>
            {
                e.HasKey(s => s.StudentId);

                e.Property(s => s.StudentNumber)
                    .IsRequired()
                    .HasMaxLength(30);

                e.HasIndex(s => s.StudentNumber)
                    .IsUnique();

                e.Property(s => s.GradeLevel)
                    .IsRequired()
                    .HasMaxLength(20);

                e.Property(s => s.Section)
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(s => s.SchoolYear)
                    .IsRequired()
                    .HasMaxLength(20);

                e.Property(s => s.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                e.HasIndex(s => s.UserId)
                    .IsUnique();

                e.HasOne(s => s.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =====================================================
            // TEACHER
            // =====================================================
            modelBuilder.Entity<Teacher>(e =>
            {
                e.HasKey(t => t.TeacherId);

                e.Property(t => t.EmployeeNumber)
                    .IsRequired()
                    .HasMaxLength(30);

                e.HasIndex(t => t.EmployeeNumber)
                    .IsUnique();

                e.Property(t => t.Department)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(t => t.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                e.HasIndex(t => t.UserId)
                    .IsUnique();

                e.HasOne(t => t.User)
                    .WithOne(u => u.Teacher)
                    .HasForeignKey<Teacher>(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =====================================================
            // AUTHORIZED EMAIL DOMAIN
            // =====================================================
            modelBuilder.Entity<AuthorizedEmailDomain>(e =>
            {
                e.HasKey(d => d.DomainId);

                e.Property(d => d.Domain)
                    .IsRequired()
                    .HasMaxLength(100);

                e.HasIndex(d => d.Domain)
                    .IsUnique();

                e.Property(d => d.InstitutionName)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            // =====================================================
            // EVALUATION TABLES (Schema: Eval)
            // =====================================================
            // =====================================================
            // EVALUATION FORM
            // =====================================================
            modelBuilder.Entity<EvaluationForm>(e =>
            {
                e.ToTable("EvaluationForm", "Eval");
                e.HasKey(x => x.FormId);
                e.Property(x => x.FormId).ValueGeneratedOnAdd();
                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
                e.Property(x => x.Code).IsRequired().HasMaxLength(20);
                e.Property(x => x.GradeBand).IsRequired().HasMaxLength(30);
                e.Property(x => x.TargetType).IsRequired().HasMaxLength(30);
                e.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<TeacherEvaluationCategory>(e =>
            {
                e.ToTable("TeacherEvaluationCategory", "Eval");
                e.HasKey(x => x.Recno);
                e.Property(x => x.Recno).ValueGeneratedOnAdd();
                e.Property(x => x.EvalType).HasMaxLength(50);
                e.Property(x => x.CatRn).HasMaxLength(10);
                e.Property(x => x.CatName).HasMaxLength(150);
                e.Property(x => x.CatRate).HasColumnType("decimal(18,2)");

                // New relationship
                e.HasOne(x => x.Form)
                 .WithMany(f => f.Categories)
                 .HasForeignKey(x => x.FormId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TeacherEvaluationResult>(e =>
            {
                e.ToTable("TeacherEvaluationResult", "Eval");
                e.HasKey(x => x.Recno);
                e.Property(x => x.Recno).ValueGeneratedOnAdd();
                e.Property(x => x.EvalType).HasMaxLength(50);
                e.Property(x => x.QnName).HasColumnType("nvarchar(max)");

                // New relationship
                e.HasOne(x => x.Form)
                 .WithMany()
                 .HasForeignKey(x => x.FormId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GuidanceEvaluationResult>(e =>
            {
                e.ToTable("GuidanceEvaluationResult", "Eval");
                e.HasKey(x => x.Recno);
                e.Property(x => x.Recno).ValueGeneratedOnAdd();
                e.Property(x => x.StudentId).HasMaxLength(50);
                e.Property(x => x.Sy).HasMaxLength(9);
            });

            modelBuilder.Entity<GuidanceEvaluationLog>(e =>
            {
                e.ToTable("GuidanceEvaluationLog", "Eval");
                e.HasKey(x => x.Recno);
                e.Property(x => x.Recno).ValueGeneratedOnAdd();
                e.Property(x => x.StudentId).HasMaxLength(10);
                e.Property(x => x.Sy).HasMaxLength(9);
            });

            modelBuilder.Entity<TeacherEvaluationStrength>(e =>
            {
                e.ToTable("TeacherEvaluationStrength", "Eval");
                e.HasKey(x => x.Recno);
                e.Property(x => x.Recno).ValueGeneratedOnAdd();
                e.Property(x => x.EvalType).HasMaxLength(50);
                e.Property(x => x.TeacherId).HasMaxLength(50);
                e.Property(x => x.StudentId).HasMaxLength(50);
                e.Property(x => x.Sy).HasMaxLength(50);
                e.Property(x => x.CommentName).HasColumnType("nvarchar(max)");
            });

            // =====================================================
            // SEED DATA
            // =====================================================

            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "System Administrator",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Role
                {
                    Id = 2,
                    Name = "Teacher",
                    NormalizedName = "TEACHER",
                    Description = "Classroom Teacher",
                    IsActive = true,
                    SortOrder = 2,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Role
                {
                    Id = 3,
                    Name = "Student",
                    NormalizedName = "STUDENT",
                    Description = "Student",
                    IsActive = true,
                    SortOrder = 3,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // Authorized Domain
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

            // Evaluation Categories
            modelBuilder.Entity<TeacherEvaluationCategory>().HasData(
                new TeacherEvaluationCategory { Recno = 1, EvalType = "Teacher Evaluation", CatNo = 1, CatRn = "A", CatName = "Teaching Effectiveness", CatRate = 25.00m },
                new TeacherEvaluationCategory { Recno = 2, EvalType = "Teacher Evaluation", CatNo = 2, CatRn = "B", CatName = "Classroom Management", CatRate = 20.00m },
                new TeacherEvaluationCategory { Recno = 3, EvalType = "Teacher Evaluation", CatNo = 3, CatRn = "C", CatName = "Communication Skills", CatRate = 20.00m },
                new TeacherEvaluationCategory { Recno = 4, EvalType = "Teacher Evaluation", CatNo = 4, CatRn = "D", CatName = "Subject Knowledge", CatRate = 20.00m },
                new TeacherEvaluationCategory { Recno = 5, EvalType = "Teacher Evaluation", CatNo = 5, CatRn = "E", CatName = "Student Engagement", CatRate = 15.00m }
            );

            // Evaluation Questions
            modelBuilder.Entity<TeacherEvaluationResult>().HasData(
                // Category 1
                new TeacherEvaluationResult { Recno = 1, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 1, QnName = "The teacher explains lessons clearly and in an organized manner." },
                new TeacherEvaluationResult { Recno = 2, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 2, QnName = "The teacher uses appropriate examples and illustrations." },
                new TeacherEvaluationResult { Recno = 3, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 3, QnName = "The teacher adjusts teaching methods to different learning styles." },
                new TeacherEvaluationResult { Recno = 4, EvalType = "Teacher Evaluation", CatNo = 1, QnNo = 4, QnName = "The teacher provides timely and constructive feedback on student work." },

                // Category 2
                new TeacherEvaluationResult { Recno = 5, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 1, QnName = "The teacher maintains an orderly and respectful classroom environment." },
                new TeacherEvaluationResult { Recno = 6, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 2, QnName = "The teacher manages time effectively during class periods." },
                new TeacherEvaluationResult { Recno = 7, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 3, QnName = "The teacher handles student misbehavior fairly and consistently." },
                new TeacherEvaluationResult { Recno = 8, EvalType = "Teacher Evaluation", CatNo = 2, QnNo = 4, QnName = "Classroom rules and expectations are clearly communicated." },

                // Category 3
                new TeacherEvaluationResult { Recno = 9, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 1, QnName = "The teacher communicates expectations and instructions clearly." },
                new TeacherEvaluationResult { Recno = 10, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 2, QnName = "The teacher listens to and responds to student questions." },
                new TeacherEvaluationResult { Recno = 11, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 3, QnName = "The teacher uses language appropriate to the students' level." },
                new TeacherEvaluationResult { Recno = 12, EvalType = "Teacher Evaluation", CatNo = 3, QnNo = 4, QnName = "The teacher provides opportunities for students to express ideas." },

                // Category 4
                new TeacherEvaluationResult { Recno = 13, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 1, QnName = "The teacher demonstrates thorough knowledge of the subject matter." },
                new TeacherEvaluationResult { Recno = 14, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 2, QnName = "The teacher relates the subject to real-life situations." },
                new TeacherEvaluationResult { Recno = 15, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 3, QnName = "The teacher stays updated with current developments in the field." },
                new TeacherEvaluationResult { Recno = 16, EvalType = "Teacher Evaluation", CatNo = 4, QnNo = 4, QnName = "The teacher answers content-related questions accurately." },

                // Category 5
                new TeacherEvaluationResult { Recno = 17, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 1, QnName = "The teacher encourages active participation from all students." },
                new TeacherEvaluationResult { Recno = 18, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 2, QnName = "The teacher creates a motivating and supportive learning atmosphere." },
                new TeacherEvaluationResult { Recno = 19, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 3, QnName = "The teacher uses varied activities to sustain student interest." },
                new TeacherEvaluationResult { Recno = 20, EvalType = "Teacher Evaluation", CatNo = 5, QnNo = 4, QnName = "The teacher recognizes and praises student effort and achievement." }
            );

            // Evaluation Forms
            modelBuilder.Entity<EvaluationForm>().HasData(
                new EvaluationForm
                {
                    FormId = 1,
                    Name = "Preschool - Grade 3 Teacher Evaluation",
                    Code = "PRE-G3",
                    GradeBand = "Preschool-G3",
                    TargetType = "SubjectTeacher",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new EvaluationForm
                {
                    FormId = 2,
                    Name = "Grades 4 - 6 Teacher Evaluation",
                    Code = "G4-G6",
                    GradeBand = "G4-G6",
                    TargetType = "SubjectTeacher",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new EvaluationForm
                {
                    FormId = 3,
                    Name = "Junior High & Senior High Teacher Evaluation",
                    Code = "JHS-SHS",
                    GradeBand = "JHS-SHS",
                    TargetType = "SubjectTeacher",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // =====================================================
            // TEST USERS + STUDENTS (from legacy DMCCFI data)
            // =====================================================

            // 1. Create 3 simple test users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "jelah.cuevas@dmc.edu.ph",
                    NormalizedUserName = "JELAH.CUEVAS@DMC.EDU.PH",
                    Email = "jelah.cuevas@dmc.edu.ph",
                    NormalizedEmail = "JELAH.CUEVAS@DMC.EDU.PH",
                    EmailConfirmed = true,
                    GoogleSubjectId = "test-google-sub-001",
                    FirstName = "Jelah",
                    LastName = "Cuevas",
                    Status = "Active",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 2,
                    UserName = "shevonn.salasala@dmc.edu.ph",
                    NormalizedUserName = "SHEVONN.SALASALA@DMC.EDU.PH",
                    Email = "shevonn.salasala@dmc.edu.ph",
                    NormalizedEmail = "SHEVONN.SALASALA@DMC.EDU.PH",
                    EmailConfirmed = true,
                    GoogleSubjectId = "test-google-sub-002",
                    FirstName = "Shevonn",
                    LastName = "Salasala",
                    Status = "Active",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    Id = 3,
                    UserName = "chaser.gone@dmc.edu.ph",
                    NormalizedUserName = "CHASER.GONE@DMC.EDU.PH",
                    Email = "chaser.gone@dmc.edu.ph",
                    NormalizedEmail = "CHASER.GONE@DMC.EDU.PH",
                    EmailConfirmed = true,
                    GoogleSubjectId = "test-google-sub-003",
                    FirstName = "Chaser",
                    LastName = "Gone",
                    Status = "Active",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                }
            );

            // 2. Link them to Student records (using the real student numbers from the old SQL)
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentId = 1,
                    UserId = 1,
                    StudentNumber = "2020-0331",          // from old SQL
                    GradeLevel = "Grade 6",
                    Section = "GRACE",
                    SchoolYear = "2025-2026",
                    Status = "Active"
                },
                new Student
                {
                    StudentId = 2,
                    UserId = 2,
                    StudentNumber = "2025-2219",          // from old SQL
                    GradeLevel = "Grade 7",
                    Section = "St. Solomon Leclerq",
                    SchoolYear = "2025-2026",
                    Status = "Active"
                },
                new Student
                {
                    StudentId = 3,
                    UserId = 3,
                    StudentNumber = "2025-2235",          // from old SQL
                    GradeLevel = "Kindergarten",
                    Section = "KINDERGARTEN",
                    SchoolYear = "2025-2026",
                    Status = "Active"
                }
            );
        }
    }
}