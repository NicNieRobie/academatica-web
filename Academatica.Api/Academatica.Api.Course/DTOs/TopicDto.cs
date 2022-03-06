using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class TopicDto
    {
        [Required]
        [RegularExpression("^[0-9]+-[0-1]:[0-9]+$", ErrorMessage = "Invalid topic ID format.")]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public bool IsAlgebraTopic { get; set; }
        [Required]
        public int CompletionRate { get; set; }
    }
}
