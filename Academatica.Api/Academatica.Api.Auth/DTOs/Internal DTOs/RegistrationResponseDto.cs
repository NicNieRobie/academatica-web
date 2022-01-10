using System.Collections.Generic;

namespace SmartMath.Api.Auth.DTOs
{
    public class RegistrationResponseDto
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}
