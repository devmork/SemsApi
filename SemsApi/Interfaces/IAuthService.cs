using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using SemsApi.DTO.Authentication;
using SemsApi.Models;
using System.Security.Claims;

namespace SemsApi.Interfaces
{
    public interface IAuthService
    {
        Task<GoogleSignInResponse> SignInWithGoogleAsync(ClaimsPrincipal? principal);
        Task<TokenResponse> IssueTokensAsync(ApplicationUser user, string? ip, CancellationToken ct = default);
        Task<TokenResponse> RefreshTokenAsync(string? refreshToken, string? ip, CancellationToken ct = default);
        Task LogoutAsync(string? refreshToken, string? ip, CancellationToken ct = default);
    }
}
