using SmartMath.Api.Auth.Models;

namespace SmartMath.Api.Auth.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
