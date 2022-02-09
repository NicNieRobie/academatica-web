using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Users.DTOs
{
    public class UserRestorePasswordResponseDto
    {
        public bool ConfirmationPending { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
