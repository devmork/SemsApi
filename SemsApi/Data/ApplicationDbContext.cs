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
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<EvaluationForm> Forms => Set<EvaluationForm>();
        public DbSet<EvaluationType> EvaluationTypes => Set<EvaluationType>();
        public DbSet<EvaluationCategory> TeacherEvaluationCategories => Set<EvaluationCategory>();
        public DbSet<EvaluationFormCategory> EvaluationFormCategories => Set<EvaluationFormCategory>();
        public DbSet<EvaluationFormQuestion> EvaluationFormQuestions => Set<EvaluationFormQuestion>();
        public DbSet<EvaluationFormQuestionAnswer> EvaluationFormQuestionAnswers => Set<EvaluationFormQuestionAnswer>();
        public DbSet<TeacherEvaluationComment> TeacherEvaluationComments => Set<TeacherEvaluationComment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<SubjectOffering> SubjectOfferings => Set<SubjectOffering>();
        public DbSet<StudentSubjectEnrollment> StudentSubjectEnrollments => Set<StudentSubjectEnrollment>();
        public DbSet<StudentTermEnrollment> StudentTermEnrollments => Set<StudentTermEnrollment>();
        public DbSet<TeacherEvaluationSubmission> TeacherEvaluationSubmissions => Set<TeacherEvaluationSubmission>();
        public DbSet<TeacherEvaluationAnswer> TeacherEvaluationAnswers => Set<TeacherEvaluationAnswer>();
        public DbSet<EvaluationSession> EvaluationSessions => Set<EvaluationSession>();
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
                e.Property(x => x.EvaluationTypeName).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.EvaluationTypeName).IsUnique();
            });

            modelBuilder.Entity<EvaluationForm>(e =>
            {
                e.ToTable("EvaluationForms", "Eval");
                e.HasKey(x => x.FormId);
                e.Property(x => x.FormId).ValueGeneratedOnAdd();

                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Description).HasMaxLength(1000);
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

            modelBuilder.Entity<EvaluationCategory>(e =>
            {
                e.ToTable("EvaluationCategory", "Eval");
                e.HasKey(x => x.CategoryId);
                e.Property(x => x.CategoryId).ValueGeneratedOnAdd();
                e.Property(x => x.CategoryName).HasMaxLength(150);
                e.Property(x => x.CategoryRate).HasColumnType("decimal(18,2)");
                e.HasIndex(x => x.EvaluationTypeId);

                e.HasOne(x => x.EvaluationType)
                 .WithMany()
                 .HasForeignKey(x => x.EvaluationTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EvaluationFormCategory>(e =>
            {
                e.ToTable("EvaluationFormCategory", "Eval");
                e.HasKey(x => x.FormCategoryId);
                e.Property(x => x.FormCategoryId).ValueGeneratedOnAdd();

                e.HasIndex(x => new { x.FormId, x.CategoryId }).IsUnique();

                e.HasOne(x => x.EvaluationForm)
                 .WithMany()
                 .HasForeignKey(x => x.FormId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.EvaluationCategory)
                 .WithMany()
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EvaluationFormQuestion>(e =>
            {
                e.ToTable("EvaluationFormQuestion", "Eval");
                e.HasKey(x => x.FormQuestionId);
                e.Property(x => x.FormQuestionId).ValueGeneratedOnAdd();
                e.Property(x => x.QuestionName).HasColumnType("nvarchar(max)");
                e.Property(x => x.Points).HasPrecision(5, 2);

                e.HasIndex(x => new { x.FormCategoryId, x.DisplayOrder }).IsUnique();

                e.HasOne(x => x.FormCategory)
                 .WithMany()
                 .HasForeignKey(x => x.FormCategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EvaluationFormQuestionAnswer>(e =>
            {
                e.ToTable("EvaluationFormQuestionAnswer", "Eval");
                e.HasKey(x => x.AnswerId);
                e.Property(x => x.AnswerId).ValueGeneratedOnAdd();
                e.Property(x => x.RateVal).HasPrecision(5, 2);

                e.HasIndex(x => new { x.SubmissionId, x.FormQuestionId }).IsUnique();

                e.HasOne(x => x.FormQuestion)
                 .WithMany()
                 .HasForeignKey(x => x.FormQuestionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TeacherEvaluationComment>(e =>
            {
                e.ToTable("TeacherEvaluationComment", "Eval");
                e.HasKey(x => x.Recno);
                e.Property(x => x.Recno).ValueGeneratedOnAdd();
                e.Property(x => x.Sy).HasMaxLength(50);
                e.Property(x => x.CommentName).HasColumnType("nvarchar(max)");
                e.HasIndex(x => x.EvaluationTypeId);

                e.HasOne(x => x.EvaluationType)
                 .WithMany()
                 .HasForeignKey(x => x.EvaluationTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Teacher).WithMany().HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
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
                e.HasOne(x => x.EvaluationForm).WithMany()
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
                e.HasIndex(x => new { x.SubmissionId, x.QuestionId }).IsUnique();
                e.HasOne(x => x.Submission).WithMany(s => s.Answers)
                    .HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Question)
                    .WithMany()
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
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

                // One non-archived session per EvaluationForm + facility + SchoolYear + Term.
                // FacilityId is part of the key so the same form can be offered to several
                // facilities in one period. Archived sessions may repeat the same period.
                e.HasIndex(x => new { x.FormId, x.FacilityId, x.SchoolYear, x.Term, x.GradeLevelFrom, x.GradeLevelTo })
                    .IsUnique()
                    .HasFilter("[Status] <> 3");

                e.HasIndex(x => new { x.Status, x.OpensAtUtc, x.ClosesAtUtc });
                e.HasIndex(x => x.CreatedBy);

                e.HasOne(x => x.EvaluationForm)
                    .WithMany()
                    .HasForeignKey(x => x.FormId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Facility)
                    .WithMany()
                    .HasForeignKey(x => x.FacilityId)
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
                e.HasIndex(x => x.FacilityName).IsUnique();
            });

            modelBuilder.Entity<FacilityEvaluationSubmission>(e =>
            {
                e.ToTable("FacilityEvaluationSubmissions", "Eval");
                e.HasKey(x => x.SubmissionId);
                e.Property(x => x.SubmissionId).ValueGeneratedOnAdd();
                e.Property(x => x.SchoolYear).IsRequired().HasMaxLength(9);
                e.Property(x => x.Term).IsRequired();

                e.HasIndex(x => new { x.StudentId, x.FacilityId, x.EvaluationSessionId })
                    .IsUnique()
                    .HasFilter("[EvaluationSessionId] IS NOT NULL");
                e.HasIndex(x => new { x.StudentId, x.FacilityId, x.SchoolYear, x.Term });
                e.HasIndex(x => x.FacilityId);
                e.HasIndex(x => x.EvaluationSessionId);

                e.HasOne(x => x.Student).WithMany()
                    .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Facility).WithMany(f => f.EvaluationSubmissions)
                    .HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EvaluationType).WithMany()
                    .HasForeignKey(x => x.EvaluationTypeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EvaluationForm).WithMany()
                    .HasForeignKey(x => x.FormId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EvaluationSession).WithMany()
                    .HasForeignKey(x => x.EvaluationSessionId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FacilityEvaluationAnswer>(e =>
            {
                e.ToTable("FacilityEvaluationAnswers", "Eval");
                e.HasKey(x => x.AnswerId);
                e.Property(x => x.AnswerId).ValueGeneratedOnAdd();
                e.HasIndex(x => new { x.SubmissionId, x.QuestionId }).IsUnique();
                e.HasOne(x => x.Submission).WithMany(s => s.Answers)
                    .HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Question)
                    .WithMany()
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
