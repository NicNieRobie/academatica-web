using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Auth.DTOs
{
    public class AuthRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid E-Mail address")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
