using SemsApi.Models;

namespace SemsApi.DTO.Authentication;

public sealed record GoogleSignInResponse(ApplicationUser User, bool IsNewUser);