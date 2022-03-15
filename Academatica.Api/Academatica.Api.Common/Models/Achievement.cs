using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class Achievement
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ImageUrl { get; set; }
    }
}
