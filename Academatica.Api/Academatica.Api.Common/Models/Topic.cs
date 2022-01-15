using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
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
        public Guid? PrecedingTopicId { get; set; }
        [Required]
        public Guid TierId { get; set; }

        public Tier PrecedingTopic { get; set; }
        public Tier Tier { get; set; }
    }
}
