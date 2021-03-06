using System;
using System.ComponentModel.DataAnnotations;

namespace Academatica.Api.Common.Models
{
    public class UserAchievement
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid AchievementId { get; set; }
        [Required]
        public DateTime AchievedAt { get; set; }
        [Required]
        public ulong AchievedAmount { get; set; }

        public User User { get; set; }
        public Achievement Achievement { get; set; }
    }
}
