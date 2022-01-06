using SmartMath.Api.Common.Models;
using System;

namespace SmartMath.Api.Users.Models
{
    public class ReceivedAchievementEntry: Achievement
    {
        public DateTime AchievedAt { get; set; }
    }
}
