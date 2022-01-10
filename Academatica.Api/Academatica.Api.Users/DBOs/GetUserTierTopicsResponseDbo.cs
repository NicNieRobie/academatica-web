using SmartMath.Api.Users.Models;
using System.Collections.Generic;

namespace SmartMath.Api.Users.DBOs
{
    public class GetUserTierTopicsResponseDbo
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<UserStatsTierTopic> TopicStats { get; set; }
    }
}
