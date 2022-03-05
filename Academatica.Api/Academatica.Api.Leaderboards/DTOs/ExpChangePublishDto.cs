using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class ExpChangePublishDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public ulong ExpThisWeek { get; set; }
        [Required]
        public string Event { get; set; }
    }
}
