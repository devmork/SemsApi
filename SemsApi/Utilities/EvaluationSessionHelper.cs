using SemsApi.DTO.EvaluationSessions;
using SemsApi.Models;
using SemsApi.Models.Eval;

namespace SemsApi.Utilities
{
    public static class EvaluationSessionHelper
    {
        public const int MaxSchoolYearLength = 9;
        public const int MaxNotesLength = 1000;

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
                FormCode = session.Form?.Code ?? string.Empty,
                FormTitle = session.Form?.Title ?? string.Empty,
                EvaluationTypeId = session.Form?.EvaluationTypeId ?? 0,
                EvaluationTypeName = session.Form?.EvaluationType?.EvaluationTypeName ?? string.Empty,
                TargetType = session.Form?.TargetType ?? string.Empty,
                ProgramId = session.Form?.ProgramId ?? 0,
                SchoolYear = session.SchoolYear,
                Term = session.Term,
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