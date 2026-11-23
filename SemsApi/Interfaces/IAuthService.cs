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

        /// <summary>Creates a single-use, short-lived code that stands in for tokens on the redirect URL.</summary>
        Task<string> CreateSignInCodeAsync(int userId, CancellationToken ct = default);

        /// <summary>Consumes a sign-in code. Returns the user id, or null when the code is unknown or already used.</summary>
        Task<int?> RedeemSignInCodeAsync(string? code, CancellationToken ct = default);

        /// <summary>Loads an active (not deactivated) user by id.</summary>
        Task<ApplicationUser?> GetActiveUserAsync(int userId, CancellationToken ct = default);
    }
}
