using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Auth.DTOs
{
    public class AuthRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
