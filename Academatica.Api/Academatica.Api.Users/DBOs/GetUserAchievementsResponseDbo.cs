using SmartMath.Api.Users.Models;
using System.Collections.Generic;

namespace SmartMath.Api.Users.DBOs
{

    public class GetUserAchievementsResponseDbo
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<ReceivedAchievementEntry> Achievements { get; set; }
    }
}
