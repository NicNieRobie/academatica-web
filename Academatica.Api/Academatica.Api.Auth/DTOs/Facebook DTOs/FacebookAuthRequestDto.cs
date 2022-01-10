using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Auth.DTOs
{
    public class FacebookAuthRequestDto
    {
        [Required]
        public string AccessToken { get; set; }
    }
}
