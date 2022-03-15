using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Course.DTOs
{
    public class AchievementDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ImageUrl { get; set; }
    }
}
