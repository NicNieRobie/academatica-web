using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class Tier
    {
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid tier ID format.")]
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
