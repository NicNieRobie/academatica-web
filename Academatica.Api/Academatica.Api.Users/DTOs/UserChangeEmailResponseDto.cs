using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class UserChangeEmailResponseDto
    {
        public bool ConfirmationPending { get; set; }
        public bool Success { get; set; }
    }
}