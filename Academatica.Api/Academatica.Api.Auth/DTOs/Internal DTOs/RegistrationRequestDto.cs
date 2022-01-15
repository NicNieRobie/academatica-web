using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Auth.DTOs
{
    public class RegistrationRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid E-Mail address")]
        public string Email { get; set; }
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Password does not match requirements")]
        public string Password { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
}
