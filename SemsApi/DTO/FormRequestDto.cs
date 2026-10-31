namespace SemsApi.DTO.Form
{
    public class FormRequestDto
    {
        /// <summary>Short unique code e.g. "PRE-G3". Required on create.</summary>
        public string? Code { get; set; }

        /// <summary>Grade band label e.g. "Preschool-G3". Optional.</summary>
        public string? GradeBand { get; set; }

        /// <summary>"SubjectTeacher" or "Facility". Optional, defaults to "SubjectTeacher".</summary>
        public string? TargetType { get; set; }

        /// <summary>0 = all, 1 = PreK-G3, 2 = G4-G6, 3 = JHS-SHS.</summary>
        public int? ProgramId { get; set; }

        public int? EvaluationTypeId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        /// <summary>1 = active, 0 = inactive. Null on create = 1. Null on update = leave unchanged.</summary>
        public int? IsActive { get; set; }
    }
}