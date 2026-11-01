using System.Diagnostics.Contracts;

namespace SemsApi.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<SubjectOffering> Offerings { get; set; } = new List<SubjectOffering>();
    }
}
