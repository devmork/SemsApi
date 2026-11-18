using SemsApi.DTO.EvaluationSessions;
using SemsApi.Models;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class EvaluationSessionHelper
    {
        public const int MaxSchoolYearLength = 9;
        public const int MaxNotesLength = 1000;
        public const int MinGradeLevel = GradeLevelHelper.MinGradeLevel;
        public const int MaxGradeLevel = GradeLevelHelper.MaxGradeLevel;

        public static string? ValidateCreate(EvaluationSessionCreateRequest request)
        {
            if (request is null)
                return "Request body is required.";

            if (request.FormId <= 0)
                return "FormId is required.";

            if (string.IsNullOrWhiteSpace(request.SchoolYear))
                return "SchoolYear is required.";

            if (request.SchoolYear.Trim().Length > MaxSchoolYearLength)
                return $"SchoolYear must not exceed {MaxSchoolYearLength} characters.";

            if (request.Term <= 0)
                return "Term must be a positive number.";

            var gradeError = ValidateGradeRange(request.GradeLevelFrom, request.GradeLevelTo);
            if (gradeError is not null)
                return gradeError;

            if (request.OpensAtUtc.HasValue &&
                request.ClosesAtUtc.HasValue &&
                request.OpensAtUtc.Value >= request.ClosesAtUtc.Value)
            {
                return "OpensAtUtc must be earlier than ClosesAtUtc.";
            }

            if (request.Notes is not null &&
                request.Notes.Trim().Length > MaxNotesLength)
            {
                return $"Notes must not exceed {MaxNotesLength} characters.";
            }

            return null;
        }

        /// <summary>
        /// Validates an inclusive grade-level range. Null bounds mean "unbounded".
        /// </summary>
        public static string? ValidateGradeRange(int? from, int? to)
        {
            if (from.HasValue && (from.Value < MinGradeLevel || from.Value > MaxGradeLevel))
                return $"GradeLevelFrom must be between {MinGradeLevel} and {MaxGradeLevel}.";

            if (to.HasValue && (to.Value < MinGradeLevel || to.Value > MaxGradeLevel))
                return $"GradeLevelTo must be between {MinGradeLevel} and {MaxGradeLevel}.";

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                return "GradeLevelFrom must not be greater than GradeLevelTo.";

            return null;
        }

        public static string? ValidateUpdate(EvaluationSessionUpdateRequest request)
        {
            if (request is null)
                return "Request body is required.";

            if (request.Notes is not null &&
                request.Notes.Trim().Length > MaxNotesLength)
            {
                return $"Notes must not exceed {MaxNotesLength} characters.";
            }

            return null;
        }

        /// <summary>
        /// A session is available to evaluators only when it is Open and the
        /// current UTC time is inside the optional window.
        /// </summary>
        public static bool IsWithinWindow(EvaluationSession session, DateTime utcNow)
        {
            if (session.Status != EvaluationSessionStatus.Open)
                return false;

            if (session.OpensAtUtc.HasValue && session.OpensAtUtc.Value > utcNow)
                return false;

            if (session.ClosesAtUtc.HasValue && session.ClosesAtUtc.Value <= utcNow)
                return false;

            return true;
        }

        /// <summary>
        /// State-machine rules for status transitions.
        /// </summary>
        public static bool CanTransition(
            EvaluationSessionStatus from,
            EvaluationSessionStatus to,
            bool allowReopen)
        {
            if (from == to)
                return true;

            return (from, to) switch
            {
                (EvaluationSessionStatus.Draft, EvaluationSessionStatus.Open) => true,
                (EvaluationSessionStatus.Draft, EvaluationSessionStatus.Closed) => true,
                (EvaluationSessionStatus.Draft, EvaluationSessionStatus.Archived) => true,
                (EvaluationSessionStatus.Open, EvaluationSessionStatus.Closed) => true,
                (EvaluationSessionStatus.Closed, EvaluationSessionStatus.Open) => allowReopen,
                (EvaluationSessionStatus.Closed, EvaluationSessionStatus.Archived) => true,
                _ => false
            };
        }

        public static EvaluationSessionDto MapToDto(
            EvaluationSession session,
            int submissionCount = 0)
        {
            return new EvaluationSessionDto
            {
                EvaluationSessionId = session.EvaluationSessionId,
                FormId = session.FormId,
                FormTitle = session.EvaluationForm?.Title ?? string.Empty,
                EvaluationTypeId = session.EvaluationForm?.EvaluationTypeId ?? 0,
                EvaluationTypeName = session.EvaluationForm?.EvaluationType?.EvaluationTypeName ?? string.Empty,
                SchoolYear = session.SchoolYear,
                Term = session.Term,
                GradeLevelFrom = session.GradeLevelFrom,
                GradeLevelTo = session.GradeLevelTo,
                OpensAtUtc = session.OpensAtUtc,
                ClosesAtUtc = session.ClosesAtUtc,
                Status = session.Status.ToString(),
                Notes = session.Notes,
                CreatedBy = session.CreatedBy,
                CreatedAtUtc = session.CreatedAtUtc,
                UpdatedAtUtc = session.UpdatedAtUtc,
                SubmissionCount = submissionCount,
                IsAvailableNow = IsWithinWindow(session, DateTime.UtcNow)
            };
        }
    }
}