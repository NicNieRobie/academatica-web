using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.Services
{
    public class LeaderboardManager : ILeaderboardManager
    {
        private readonly IDatabase _leaderboard;
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardManager(IConnectionMultiplexer connectionMultiplexer, ILeaderboardService leaderboardService)
        {
            _leaderboard = connectionMultiplexer.GetDatabase();
            _leaderboardService = leaderboardService;
        }

        public async Task UpdateLeaderboards()
        {
            HashSet<string> affectedIds = new HashSet<string>();

            foreach (var value in await _leaderboard.SortedSetRangeByRankAsync("GOLD_LEAGUE", 0, 19, Order.Descending))
            {
                if (value.HasValue && !affectedIds.Contains(value.ToString()))
                {
                    await _leaderboardService.Promote(value.ToString());
                    affectedIds.Add(value.ToString());
                }
            }

            foreach (var value in await _leaderboard.SortedSetRangeByRankAsync("SILVER_LEAGUE", 0, 19, Order.Descending))
            {
                if (value.HasValue && !affectedIds.Contains(value.ToString()))
                {
                    await _leaderboardService.Promote(value.ToString());
                    affectedIds.Add(value.ToString());
                }
            }

            foreach (var value in await _leaderboard.SortedSetRangeByRankAsync("BRONZE_LEAGUE", 0, 19, Order.Descending))
            {
                if (value.HasValue && !affectedIds.Contains(value.ToString()))
                {
                    await _leaderboardService.Promote(value.ToString());
                    affectedIds.Add(value.ToString());
                }
            }

            foreach (var value in await _leaderboard.SortedSetRangeByRankAsync("GOLD_LEAGUE", 0, 19, Order.Ascending))
            {
                if (value.HasValue && !affectedIds.Contains(value.ToString()))
                {
                    await _leaderboardService.Demote(value.ToString());
                    affectedIds.Add(value.ToString());
                }
            }

            foreach (var value in await _leaderboard.SortedSetRangeByRankAsync("SILVER_LEAGUE", 0, 19, Order.Ascending))
            {
                if (value.HasValue && !affectedIds.Contains(value.ToString()))
                {
                    await _leaderboardService.Demote(value.ToString());
                    affectedIds.Add(value.ToString());
                }
            }

            foreach (var value in await _leaderboard.SortedSetRangeByRankAsync("BRONZE_LEAGUE", 0, 19, Order.Ascending))
            {
                if (value.HasValue && !affectedIds.Contains(value.ToString()))
                {
                    await _leaderboardService.Demote(value.ToString());
                    affectedIds.Add(value.ToString());
                }
            }
        }
    }
}
