using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class Topic
    {
        [Required]
        [RegularExpression("^[0-9]+-[0-1]:[0-9]+$", ErrorMessage = "Invalid topic ID format.")]
        public string Id { get; set; }
        [Required]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid tier ID format.")]
        public string TierId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public bool IsAlgebraTopic { get; set; }

        public Tier Tier { get; set; }
    }
}
