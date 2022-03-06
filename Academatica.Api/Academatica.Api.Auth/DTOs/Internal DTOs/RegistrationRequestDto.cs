using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Auth.DTOs
{
    public class RegistrationRequestDto
    {
        /// <summary>
        /// User email (must be unique).
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Invalid E-Mail address")]
        public string Email { get; set; }

        /// <summary>
        /// Username (must be unique).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Password does not match requirements")]
        public string Password { get; set; }

        /// <summary>
        /// Password confirmation (must match the password).
        /// </summary>
        [Required]
        [Compare("Password", ErrorMessage = "Confirm password does not match password")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Profile picture.
        /// </summary>
        public IFormFile ProfilePicture { get; set; }
    }
}
