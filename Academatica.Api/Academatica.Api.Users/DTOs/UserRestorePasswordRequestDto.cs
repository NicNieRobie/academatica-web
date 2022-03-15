using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Users.DTOs
{
    public class UserRestorePasswordRequestDto
    {
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Password does not match requirements")]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match password")]
        public string ConfirmNewPassword { get; set; }
        public string ConfirmationCode { get; set; }
    }
}
