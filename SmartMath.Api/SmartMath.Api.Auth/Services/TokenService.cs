using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartMath.Api.Auth.Configuration;
using SmartMath.Api.Auth.Data;
using SmartMath.Api.Auth.DTOs;
using SmartMath.Api.Auth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthDbContext _authDbContext;
        private readonly TokenValidationParameters _jwtValidationParameters;

        public TokenService(
            IOptions<JwtConfig> options, 
            IHttpContextAccessor httpContextAccessor, 
            AuthDbContext authDbContext,
            TokenValidationParameters jwtValidationParameters)
        {
            _jwtConfig = options.Value;
            _httpContextAccessor = httpContextAccessor;
            _authDbContext = authDbContext;
            _jwtValidationParameters = jwtValidationParameters;
        }

        public async Task<AuthResponseDto> AuthWithToken(User user)
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
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.Lifespan),
                SigningCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256Signature)
            });

            var accessToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = GenerateRefreshToken(token, user);

            await _authDbContext.UserRefreshTokens.AddAsync(refreshToken);
            await _authDbContext.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Token = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public RefreshToken GenerateRefreshToken(SecurityToken accessToken, User user)
        {
            return new RefreshToken()
            {
                JwtId = accessToken.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                IssuedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Token = GenerateToken() + Guid.NewGuid()
            };
        }

        public string GenerateToken()
        {
            var randomNum = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNum);
                return Convert.ToBase64String(randomNum);
            }
        }

        public async Task<RefreshValidationResponseDto> IsValidForRefresh(RefreshRequestDto tokenRequestDto)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtFormatValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jwtValidationParameters.IssuerSigningKey,
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    RequireExpirationTime = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = "SMathIOSClient"
                };

                var jwt = jwtTokenHandler.ValidateToken(tokenRequestDto.AccessToken, jwtFormatValidationParameters, out var validatedJwt);

                if (validatedJwt is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return new RefreshValidationResponseDto
                        {
                            IsValid = false,
                            Error = "Token encryption algorithm is invalid"
                        };
                    }
                }

                var unixExpiryDate = long.Parse(jwt.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp).Value);

                DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var expiryDate = baseDate.AddSeconds(unixExpiryDate).ToUniversalTime();

                if (expiryDate > DateTime.UtcNow)
                {
                    return new RefreshValidationResponseDto
                    {
                        IsValid = false,
                        Error = "Token has not yet expired"
                    };
                }

                var storedRefreshToken = await _authDbContext.UserRefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequestDto.RefreshToken);

                if (storedRefreshToken == null)
                {
                    return new RefreshValidationResponseDto
                    {
                        IsValid = false,
                        Error = "Refresh token is invalid"
                    };
                }

                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new RefreshValidationResponseDto
                    {
                        IsValid = false,
                        Error = "Token has not yet expired"
                    };
                }

                if (storedRefreshToken.IsUsed)
                {
                    return new RefreshValidationResponseDto
                    {
                        IsValid = false,
                        Error = "Token has been used"
                    };
                }

                if (storedRefreshToken.IsRevoked)
                {
                    return new RefreshValidationResponseDto
                    {
                        IsValid = false,
                        Error = "Token has been revoked"
                    };
                }

                var jti = jwt.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (storedRefreshToken.JwtId != jti)
                {
                    return new RefreshValidationResponseDto
                    {
                        IsValid = false,
                        Error = "Refresh token doesn't match the issued access token"
                    };
                }

                storedRefreshToken.IsUsed = true;

                _authDbContext.UserRefreshTokens.Update(storedRefreshToken);
                await _authDbContext.SaveChangesAsync();

                return new RefreshValidationResponseDto
                {
                    IsValid = true,
                    InvalidatedToken = storedRefreshToken
                };
            }
            catch (Exception ex)
            {
                return new RefreshValidationResponseDto
                {
                    IsValid = false,
                    Error = "Token could not be validated"
                };
            }
        }

    }
}
