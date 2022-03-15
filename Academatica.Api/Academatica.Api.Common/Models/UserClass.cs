using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class UserClass
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string ClassId { get; set; }
        [Required]
        public DateTime CompletedAt { get; set; }

        public User User { get; set; }
        public Class Class { get; set; }
    }
}
