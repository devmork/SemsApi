using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SemsApi.Models;
using SemsApi.Models.Eval;

namespace SemsApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Business tables
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();

        // Evaluation tables
        public DbSet<Form> Forms => Set<Form>();
        public DbSet<EvaluationType> EvaluationTypes => Set<EvaluationType>();
        public DbSet<TeacherEvaluationCategory> TeacherEvaluationCategories => Set<TeacherEvaluationCategory>();
        public DbSet<TeacherEvaluationResult> TeacherEvaluationResults => Set<TeacherEvaluationResult>();
        public DbSet<GuidanceEvaluationResult> GuidanceEvaluationResults => Set<GuidanceEvaluationResult>();
        public DbSet<GuidanceEvaluationLog> GuidanceEvaluationLogs => Set<GuidanceEvaluationLog>();
        public DbSet<TeacherEvaluationStrength> TeacherEvaluationStrengths => Set<TeacherEvaluationStrength>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        // Subject / SubjectOffering / Enrollment
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<SubjectOffering> SubjectOfferings => Set<SubjectOffering>();
        public DbSet<StudentSubjectEnrollment> StudentSubjectEnrollments => Set<StudentSubjectEnrollment>();
        public DbSet<StudentTermEnrollment> StudentTermEnrollments => Set<StudentTermEnrollment>();

        // Evaluation submissions
        public DbSet<TeacherEvaluationSubmission> TeacherEvaluationSubmissions => Set<TeacherEvaluationSubmission>();
        public DbSet<TeacherEvaluationAnswer> TeacherEvaluationAnswers => Set<TeacherEvaluationAnswer>();

        //Evaluation Session
        public DbSet<EvaluationSession> EvaluationSessions => Set<EvaluationSession>();

        // Facility evaluations
        public DbSet<Facility> Facilities => Set<Facility>();
        public DbSet<FacilityEvaluationSubmission> FacilityEvaluationSubmissions => Set<FacilityEvaluationSubmission>();
        public DbSet<FacilityEvaluationAnswer> FacilityEvaluationAnswers => Set<FacilityEvaluationAnswer>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.HasIndex(t => t.TokenHash).IsUnique();
                e.Property(t => t.TokenHash).HasMaxLength(64);
                e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId);
            });

            // =====================================================
            // USER (IdentityUser)
            // =====================================================
            modelBuilder.Entity<ApplicationUser>(e =>
            {
                e.Property(u => u.GoogleSubjectId).IsRequired().HasMaxLength(255);
                e.HasIndex(u => u.GoogleSubjectId).IsUnique();

                e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                e.Property(u => u.MiddleName).HasMaxLength(100);
                e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                e.Property(u => u.PictureUrl).HasMaxLength(500);

                e.Property(u => u.Status).HasConversion<int>();
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
                e.HasIndex(t => t.TokenHash).IsUnique();
                e.HasOne(t => t.User).WithMany(u => u.RefreshTokens)
                 .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
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
            // EVALUATION TABLES (Schema: Eval)
            // =====================================================
            modelBuilder.Entity<EvaluationType>(e =>
            {
                e.ToTable("EvaluationType", "Eval");
                e.HasKey(x => x.EvaluationTypeId);
                e.Property(x => x.EvaluationTypeId).ValueGeneratedOnAdd();
                e.Property(x => x.EvaluationTypeName).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.EvaluationTypeName).IsUnique();
            });

            modelBuilder.Entity<Form>(e =>
            {
                e.ToTable("Forms", "Eval");
                e.HasKey(x => x.FormId);
                e.Property(x => x.FormId).ValueGeneratedOnAdd();

                e.Property(x => x.Code).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Code).IsUnique();

                e.Property(x => x.GradeBand).HasMaxLength(30);
                e.Property(x => x.TargetType).IsRequired().HasMaxLength(30);

                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).HasMaxLength(1000);
                e.HasIndex(x => x.ProgramId);
                e.HasIndex(x => x.EvaluationTypeId);

                e.HasOne(x => x.EvaluationType)
                 .WithMany()
                 .HasForeignKey(x => x.EvaluationTypeId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Creator)
                 .WithMany()
                 .HasForeignKey(x => x.CreatedBy)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TeacherEvaluationCategory>(e =>
            {
                e.ToTable("TeacherEvaluationCategory", "Eval");
                e.HasKey(x => x.CatNo);
                e.Property(x => x.CatNo).ValueGeneratedOnAdd();
                e.Property(x => x.CatRn).HasMaxLength(10);
                e.Property(x => x.CatName).HasMaxLength(150);
                e.Property(x => x.CatRate).HasColumnType("decimal(18,2)");
                e.HasIndex(x => x.EvaluationTypeId);

                e.HasOne(x => x.EvaluationType)
                 .WithMany()
                 .HasForeignKey(x => x.EvaluationTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TeacherEvaluationResult>(e =>
            {
                e.ToTable("TeacherEvaluationResult", "Eval");
                e.HasKey(x => x.QnNo);
                e.Property(x => x.QnNo).ValueGeneratedOnAdd();
                e.Property(x => x.QnName).HasColumnType("nvarchar(max)");
                e.HasIndex(x => x.CatNo);

                e.HasOne(x => x.Category)
                 .WithMany()
                 .HasForeignKey(x => x.CatNo)
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
                e.Property(x => x.TeacherId).HasMaxLength(50);
                e.Property(x => x.StudentId).HasMaxLength(50);
                e.Property(x => x.Sy).HasMaxLength(50);
                e.Property(x => x.CommentName).HasColumnType("nvarchar(max)");
                e.HasIndex(x => x.EvaluationTypeId);

                e.HasOne(x => x.EvaluationType)
                 .WithMany()
                 .HasForeignKey(x => x.EvaluationTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // =====================================================
            // SUBJECT / SUBJECT OFFERING / ENROLLMENT
            // =====================================================
            modelBuilder.Entity<Subject>(e =>
            {
                e.HasKey(x => x.SubjectId);
                e.Property(x => x.Code).IsRequired().HasMaxLength(100);
                e.Property(x => x.Description).IsRequired().HasMaxLength(300);
                e.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<SubjectOffering>(e =>
            {
                e.HasKey(x => x.SubjectOfferingId);
                e.Property(x => x.ProgramCode).IsRequired().HasMaxLength(300);
                e.Property(x => x.EdpCode).IsRequired().HasMaxLength(500);
                e.Property(x => x.Section).IsRequired().HasMaxLength(400);
                e.Property(x => x.SchoolYear).IsRequired().HasMaxLength(9);
                e.Property(x => x.Term).IsRequired();

                // One edpcode per SY/term
                e.HasIndex(x => new { x.EdpCode, x.Section, x.SchoolYear, x.Term }).IsUnique();
                e.HasIndex(x => x.TeacherId);
                e.HasIndex(x => new { x.SchoolYear, x.Term });

                e.HasOne(x => x.Subject).WithMany(s => s.Offerings)
                    .HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Teacher).WithMany(t => t.SubjectOfferings)
                    .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Adviser).WithMany()
                    .HasForeignKey(x => x.AdviserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StudentSubjectEnrollment>(e =>
            {
                e.HasKey(x => x.EnrollmentId);
                e.Property(x => x.Status).IsRequired().HasMaxLength(20);
                e.HasIndex(x => new { x.StudentId, x.SubjectOfferingId }).IsUnique();

                e.HasOne(x => x.Student).WithMany(s => s.SubjectEnrollments)
                    .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.SubjectOffering).WithMany(o => o.Enrollments)
                    .HasForeignKey(x => x.SubjectOfferingId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StudentTermEnrollment>(e =>
            {
                e.HasKey(x => x.StudentTermEnrollmentId);
                e.Property(x => x.SchoolYear).IsRequired().HasMaxLength(9);
                e.Property(x => x.CourseCode).HasMaxLength(300);
                e.Property(x => x.Status).IsRequired().HasMaxLength(20);
                e.HasIndex(x => new { x.StudentId, x.SchoolYear, x.Term }).IsUnique();

                e.HasOne(x => x.Student).WithMany(s => s.TermEnrollments)
                    .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
            });

            // =====================================================
            // TEACHER EVALUATION SUBMISSION / ANSWER
            // =====================================================
            modelBuilder.Entity<TeacherEvaluationSubmission>(e =>
            {
                e.ToTable("TeacherEvaluationSubmissions", "Eval"); // <--- ADDED
                e.HasKey(x => x.SubmissionId);
                e.Property(x => x.SubmissionId).ValueGeneratedOnAdd(); // <--- ADDED
                e.Property(x => x.SchoolYear).IsRequired().HasMaxLength(9);

                e.HasIndex(x => new { x.StudentId, x.SubjectOfferingId }).IsUnique();
                e.HasIndex(x => new { x.StudentId, x.SchoolYear, x.Term });

                e.HasOne(x => x.Student).WithMany(s => s.EvaluationSubmissions)
                    .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.SubjectOffering).WithMany()
                    .HasForeignKey(x => x.SubjectOfferingId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Teacher).WithMany(t => t.EvaluationSubmissions)
                    .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EvaluationType).WithMany()
                    .HasForeignKey(x => x.EvaluationTypeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Form).WithMany()
                    .HasForeignKey(x => x.FormId).OnDelete(DeleteBehavior.Restrict);
                e.Property(x => x.EvaluationSessionId).IsRequired(false);
                e.HasIndex(x => x.EvaluationSessionId);

                e.HasOne(x => x.EvaluationSession)
                    .WithMany(s => s.Submissions)
                    .HasForeignKey(x => x.EvaluationSessionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TeacherEvaluationAnswer>(e =>
            {
                e.ToTable("TeacherEvaluationAnswers", "Eval"); // <--- ADDED
                e.HasKey(x => x.AnswerId);
                e.Property(x => x.AnswerId).ValueGeneratedOnAdd(); // <--- ADDED
                e.HasIndex(x => new { x.SubmissionId, x.QnNo }).IsUnique();
                e.HasOne(x => x.Submission).WithMany(s => s.Answers)
                    .HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EvaluationSession>(e =>
            {
                e.ToTable("EvaluationSessions", "Eval");
                e.HasKey(x => x.EvaluationSessionId);
                e.Property(x => x.EvaluationSessionId).ValueGeneratedOnAdd();

                e.Property(x => x.SchoolYear).IsRequired().HasMaxLength(9);
                e.Property(x => x.Term).IsRequired();
                e.Property(x => x.Status).HasConversion<int>().IsRequired();
                e.Property(x => x.Notes).HasMaxLength(1000);

                // One non-archived session per Form + SchoolYear + Term.
                // Archived sessions are historical and may repeat the same period.
                e.HasIndex(x => new { x.FormId, x.SchoolYear, x.Term })
                    .IsUnique()
                    .HasFilter("[Status] <> 3");

                e.HasIndex(x => new { x.Status, x.OpensAtUtc, x.ClosesAtUtc });
                e.HasIndex(x => x.CreatedBy);

                e.HasOne(x => x.Form)
                    .WithMany()
                    .HasForeignKey(x => x.FormId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Creator)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =====================================================
            // FACILITY / NON-TEACHER EVALUATION
            // =====================================================
            modelBuilder.Entity<Facility>(e =>
            {
                e.HasKey(x => x.FacilityId);
                e.Property(x => x.FacilityName).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).HasMaxLength(1000);
                e.Property(x => x.Location).HasMaxLength(200);
                e.HasIndex(x => x.FacilityName).IsUnique();
            });

            modelBuilder.Entity<FacilityEvaluationSubmission>(e =>
            {
                e.ToTable("FacilityEvaluationSubmissions", "Eval");
                e.HasKey(x => x.SubmissionId);
                e.Property(x => x.SubmissionId).ValueGeneratedOnAdd();
                e.Property(x => x.SchoolYear).IsRequired().HasMaxLength(9);
                e.Property(x => x.Term).IsRequired();

                e.HasIndex(x => new { x.StudentId, x.FacilityId, x.SchoolYear, x.Term }).IsUnique();
                e.HasIndex(x => x.FacilityId);
                e.HasIndex(x => x.EvaluationSessionId);

                e.HasOne(x => x.Student).WithMany()
                    .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Facility).WithMany(f => f.EvaluationSubmissions)
                    .HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EvaluationType).WithMany()
                    .HasForeignKey(x => x.EvaluationTypeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Form).WithMany()
                    .HasForeignKey(x => x.FormId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EvaluationSession).WithMany()
                    .HasForeignKey(x => x.EvaluationSessionId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FacilityEvaluationAnswer>(e =>
            {
                e.ToTable("FacilityEvaluationAnswers", "Eval");
                e.HasKey(x => x.AnswerId);
                e.Property(x => x.AnswerId).ValueGeneratedOnAdd();
                e.HasIndex(x => new { x.SubmissionId, x.QnNo }).IsUnique();
                e.HasOne(x => x.Submission).WithMany(s => s.Answers)
                    .HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}