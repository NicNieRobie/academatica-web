using SmartMath.Api.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMath.Api.Users.Models
{
    public class UserStatsTier
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int FinishedPercentage { get; set; }
        public bool IsFinished { get; set; }
    }
}
