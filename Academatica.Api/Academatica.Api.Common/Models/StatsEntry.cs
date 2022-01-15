using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Academatica.Api.Common.Models
{
    public class StatsEntry
    {
        [Required]
        [Key]
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        [Required]
        public ulong UserExp { get; set; }
        [Required]
        public uint BuoysLeft { get; set; }
        [Required]
        public ulong DaysStreak { get; set; }
        public DateTime? LastClassFinishedAt { get; set; }

        public User User { get; set; }
    }
}
