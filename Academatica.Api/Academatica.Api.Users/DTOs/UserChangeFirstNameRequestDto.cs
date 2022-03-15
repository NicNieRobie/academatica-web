using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class UserChangeFirstNameRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name should be non-empty")]
        public string FirstName { get; set; }
    }
}
