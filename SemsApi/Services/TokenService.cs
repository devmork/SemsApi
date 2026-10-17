using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SemsApi.DTO.Authentication;
using SemsApi.Interfaces;
using SemsApi.Models;
using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SemsApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string jwtToken, DateTime expiresAtUtc) GenerateAccessToken(ApplicationUser applicationUser, IEnumerable<string> roles)
        {

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, applicationUser.Id.ToString()),
                new(JwtRegisteredClaimNames.Name, applicationUser.UserName ?? ""),
                new(JwtRegisteredClaimNames.Email, applicationUser.Email ?? ""),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), DateTime.UtcNow.AddMinutes(15));

        }

        public GeneratedRefreshTsoken GenerateRefreshToken()
        {
            var raw = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));
            return new(raw, Hash(raw), DateTime.UtcNow.AddDays(7));
        }
        public string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    }
}