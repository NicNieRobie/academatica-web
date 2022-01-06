using Microsoft.IdentityModel.Tokens;
using SmartMath.Api.Auth.DTOs;
using SmartMath.Api.Common.Models;
using SmartMath.Api.Common.Models.Tokens;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Services.Interfaces
{
    public interface ITokenService
    {
        Task<AuthResponseDto> AuthWithToken(User user);
        RefreshToken GenerateRefreshToken(SecurityToken accessToken, User user);
        public Task<RefreshValidationResponseDto> IsValidForRefresh(RefreshRequestDto tokenRequestDto);
    }
}
