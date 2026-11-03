namespace SemsApi.DTO.Authentication;

public sealed record StudentProfileSubjectDto(
    int SubjectOfferingId,
    int SubjectId,
    string SubjectCode,
    string SubjectDescription,
    string GradeLevel,      // human-readable, from SubjectOffering.GradeLevel
    string Section,
    string SchoolYear,
    int Term,
    StudentProfileTeacherDto? Teacher,
    string EnrollmentStatus);