namespace SemsApi.DTO.Authentication
{
    public sealed record UserResponse(
        int Id,
        string Email,
        string FirstName,
        string LastName,
        string? PictureUrl,
        string? Role);
}
