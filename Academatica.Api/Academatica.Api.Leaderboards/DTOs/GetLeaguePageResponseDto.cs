using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.DTOs
{
    public class GetLeaguePageResponseDto
    {
        public IEnumerable<LeaderboardEntryDto> Leaderboard { get; set; }
    }
}
