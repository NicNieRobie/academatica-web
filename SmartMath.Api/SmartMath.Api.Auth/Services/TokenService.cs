using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartMath.Api.Auth.Configuration;
using SmartMath.Api.Auth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IOptions<JwtConfig> options, IHttpContextAccessor httpContextAccessor)
        {
            _jwtConfig = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateAccessToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secret = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Key));

            string authHostUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            var token = jwtTokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
                }),
                IssuedAt = DateTime.UtcNow,
                Issuer = authHostUrl,
                Audience = "SMathIOSClient",
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha512Signature)
            });

            return jwtTokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }
    }
}
