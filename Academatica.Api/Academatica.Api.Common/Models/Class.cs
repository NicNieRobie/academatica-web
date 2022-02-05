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
    }
}
