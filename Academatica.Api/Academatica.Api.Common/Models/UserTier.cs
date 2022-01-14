using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMath.Api.Common.Models
{
    public class UserTier
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid TierId { get; set; }
        [Required]
        public DateTime CompletedAt { get; set; }

        public User User { get; set; }
        public Tier Tier { get; set; }
    }
}
