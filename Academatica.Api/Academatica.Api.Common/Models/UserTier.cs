using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academatica.Api.Common.Models
{
    public class UserTier
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string TierId { get; set; }
        [Required]
        public DateTime CompletedAt { get; set; }

        public User User { get; set; }
        public Tier Tier { get; set; }
    }
}
