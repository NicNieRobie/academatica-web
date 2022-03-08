using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.DTOs
{
    public class LeaderboardEntryDto
    {
        public long Rank { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public ulong ExpThisWeek { get; set; }
        public string ProfilePic { get; set; }
        public Guid UserId { get; set; }
    }
}
