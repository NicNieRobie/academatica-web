using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Users.DTOs
{
    public class UserChangePasswordResponseDto
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
