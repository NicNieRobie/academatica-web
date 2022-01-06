﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Common.Models
{
    public class Topic
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool IsAlgebraTopic { get; set; }
        public string ImageUrl { get; set; }

        [Required]
        public Guid TierId { get; set; }
        public Tier Tier { get; set; }
    }
}
