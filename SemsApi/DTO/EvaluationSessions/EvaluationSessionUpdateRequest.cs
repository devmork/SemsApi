using SemsApi.Models;

namespace SemsApi.DTO.EvaluationSessions
{
    public class EvaluationSessionUpdateRequest
    {
        /// <summary>Optional status transition. Null means leave status unchanged.</summary>
        public EvaluationSessionStatus? Status { get; set; }

        /// <summary>Optional new opening time. Null means leave unchanged.</summary>
        public DateTime? OpensAtUtc { get; set; }

        /// <summary>Optional new closing time. Null means leave unchanged.</summary>
        public DateTime? ClosesAtUtc { get; set; }

        /// <summary>Optional notes. Empty string clears notes.</summary>
        public string? Notes { get; set; }
    }
}
