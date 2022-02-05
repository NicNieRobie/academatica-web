using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class UserChangeEmailRequestDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid E-Mail address")]
        public string EMail { get; set; }
        public string ConfirmationCode { get; set; }
    }
}
