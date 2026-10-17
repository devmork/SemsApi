using SemsApi.DTO.Authentication;
using SemsApi.Models;

namespace SemsApi.Interfaces
{
    public interface ITokenService
    {
        (string jwtToken, DateTime expiresAtUtc) GenerateAccessToken(ApplicationUser applicationUser, IEnumerable<string> roles);
        GeneratedRefreshTsoken GenerateRefreshToken();
        string Hash(string token);
    }
}