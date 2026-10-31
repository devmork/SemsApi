using SemsApi.Models;

public sealed record GoogleSignInResponse(ApplicationUser applicationUser, bool IsNewUser);