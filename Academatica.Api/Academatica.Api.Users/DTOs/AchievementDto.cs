using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Users.DTOs
{
    public class AchievementDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public ulong AchievedAmount { get; set; }
    }
}
