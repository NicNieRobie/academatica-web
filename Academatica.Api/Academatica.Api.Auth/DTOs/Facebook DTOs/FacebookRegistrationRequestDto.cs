using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Auth.DTOs
{
    public class FacebookRegistrationRequestDto : FacebookAuthRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
    }
}
