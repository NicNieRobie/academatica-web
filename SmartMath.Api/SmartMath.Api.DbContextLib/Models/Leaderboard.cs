using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMath.Api.Common.Models
{
    public class LeaderboardEntry
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public ulong ExpThisWeek { get; set; }
        [Required]
        public string League { get; set; }
    }
}
