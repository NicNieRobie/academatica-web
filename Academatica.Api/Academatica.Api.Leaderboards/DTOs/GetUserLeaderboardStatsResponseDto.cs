using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.DTOs
{
    public class GetUserLeaderboardStatsResponseDto
    {
        public string League { get; set; }
        public long Rank { get; set; }
    }
}
