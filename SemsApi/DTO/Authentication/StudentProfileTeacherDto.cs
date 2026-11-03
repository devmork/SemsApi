namespace SemsApi.DTO.Authentication;

public sealed record StudentProfileTeacherDto(
    int TeacherId,
    string? EmployeeNumber,
    string FullName);