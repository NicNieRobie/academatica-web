using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Users.Models
{
    public class UserStatsTierTopic
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
        public bool IsFinished { get; set; }
    }
}
