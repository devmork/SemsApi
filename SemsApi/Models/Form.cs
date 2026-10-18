namespace SemsApi.Models.Eval
{
    /// <summary>
    /// A form (e.g. the Teacher Evaluation form for Grade 4 to 6, or the Canteen Evaluation form).
    /// Its categories and questions are the categories (and their questions) that belong to its EvaluationType.
    /// </summary>
    public class Form
    {
        public int FormId { get; set; }

        /// <summary>0 = all students, 1 = Preschool to Grade 3, 2 = Grade 4 to 6, 3 = JHS to SHS. Decides which students get the form.</summary>
        public int ProgramId { get; set; }

        /// <summary>FK to EvaluationType - decides which categories and questions the form contains.</summary>
        public int EvaluationTypeId { get; set; }
        public EvaluationType EvaluationType { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>FK to Users (AspNetUsers.Id) - the user who created the form.</summary>
        public int CreatedBy { get; set; }
        public User Creator { get; set; } = null!;

        /// <summary>1 = active, 0 = inactive/draft.</summary>
        public int IsActive { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}