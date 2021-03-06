using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class Class
    {
        [Required]
        [RegularExpression("^[0-9]+-[0-1]:[0-9]+-[0-9]+$", ErrorMessage = "Invalid class ID format.")]
        public string Id { get; set; }
        [Required]
        [RegularExpression("^[0-9]+-[0-1]:[0-9]+$", ErrorMessage = "Invalid topic ID format.")]
        public string TopicId { get; set; }
        [Required]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Invalid tier ID format.")]
        public string TierId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public ulong ExpReward { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public string TheoryUrl { get; set; }
        [Required]
        public uint ProblemNum { get; set; }
        [Required]
        public bool IsAlgebraClass { get; set; }

        public Topic Topic { get; set; }
        public Tier Tier { get; set; }
    }
}
