namespace SemsApi.DTO.Authentication;

/// <summary>
/// The student-specific slice of the authenticated user's profile.
/// Returned by <c>GET /api/auth/me</c> only when the account's primary role is "Student"
/// and a matching <c>Student</c> row exists. Otherwise <c>null</c>.
/// </summary>
public sealed record StudentProfileDto(
    int StudentId,
    string StudentNumber,
    string? GradeLevel,     // human-readable, from current StudentTermEnrollment
    string? Section,
    string? SchoolYear,
    int? Term,
    string Status,
    IReadOnlyList<StudentProfileSubjectDto> Subjects);