using SemsApi.Models;

namespace SemsApi.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
