using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Common.Models
{
    public class Class
    {
        [Required]
        public Guid Id { get; set; }
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
        public Guid TierId { get; set; }
        public Tier Tier { get; set; }
        [Required]
        public Guid TopicId { get; set; }
        public Topic Topic { get; set; }
    }
}
