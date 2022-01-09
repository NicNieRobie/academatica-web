using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Common.Models
{
    public class Tier
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Guid? PrecedingTierId { get; set; }

        public Tier PrecedingTier { get; set; }
    }
}
