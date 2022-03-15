using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class UserChangeUsernameRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username should be non-empty")]
        public string Username { get; set; }
    }
}
