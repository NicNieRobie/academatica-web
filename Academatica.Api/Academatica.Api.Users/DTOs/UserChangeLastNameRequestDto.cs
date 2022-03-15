using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Users.DTOs
{
    public class UserChangeLastNameRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name should be non-empty")]
        public string LastName { get; set; }
    }
}
