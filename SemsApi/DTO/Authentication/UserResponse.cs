namespace SemsApi.DTO.Authentication
{
    public sealed record UserResponse(
        int Id,
        string Email,
        string FirstName,
        string LastName,
        string FullName,
        string? PictureUrl,
        string? Role,
        StudentProfileDto? StudentProfile);
}
