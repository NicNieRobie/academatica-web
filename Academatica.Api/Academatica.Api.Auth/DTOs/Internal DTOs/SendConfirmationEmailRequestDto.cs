using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Academatica.Api.Common.Models;

namespace Academatica.Api.Auth.DTOs
{
    public class SendConfirmationEmailRequestDto
    {
        [Required]
        public User User { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid E-Mail address")]
        public string Email { get; set; }
        [Required]
        public bool IsEmailChange { get; set; }
    }
}
