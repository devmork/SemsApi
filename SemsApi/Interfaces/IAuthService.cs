using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Authentication;
using SemsApi.DTO.ApplicationUser;
using SemsApi.Models;
using System.Security.Claims;

namespace SemsApi.Interfaces
{
    public interface IAuthService
    {
        Task<GoogleSignInResponse> SignInWithGoogleAsync(ClaimsPrincipal? principal);
        Task<AuthTokenResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct = default);
        Task<AuthTokenResponse> RefreshTokenAsync(string? refreshToken, CancellationToken ct = default);
        Task LogoutAsync(string? refreshToken, CancellationToken ct = default);
        Task<UserResponse?> GetCurrentUserAsync(int userId, CancellationToken ct = default);
    }
}
