using SmartMath.Api.Common.Models.Tokens;

namespace SmartMath.Api.Auth.DTOs
{
    public class RefreshValidationResponseDto
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }
        public RefreshToken InvalidatedToken { get; set; }
    }
}
