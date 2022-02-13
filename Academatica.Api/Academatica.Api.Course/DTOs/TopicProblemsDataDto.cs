using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class TopicProblemsDataDto
    {
        [Required]
        [Range(1, 3, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public uint Difficulty { get; set; }
        [Required]
        public int Count { get; set; }
    }
}
