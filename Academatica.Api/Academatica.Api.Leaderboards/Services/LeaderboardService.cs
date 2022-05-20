using Academatica.Api.Common.Data;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IDatabase _leaderboard;
        private readonly AcadematicaDbContext _academaticaDbContext;

        public LeaderboardService(IConnectionMultiplexer connectionMultiplexer, AcadematicaDbContext academaticaDbContext)
        {
            _leaderboard = connectionMultiplexer.GetDatabase();
            _academaticaDbContext = academaticaDbContext;
        }

        public async Task<string> GetUserLeague(string userId)
        {
            var score = await _leaderboard.SortedSetScoreAsync("GOLD_LEAGUE", userId.ToString());

            if (score != null)
            {
                return "gold";
            }

            score = await _leaderboard.SortedSetScoreAsync("SILVER_LEAGUE", userId.ToString());

            if (score != null)
            {
                return "silver";
            }

            score = await _leaderboard.SortedSetScoreAsync("BRONZE_LEAGUE", userId.ToString());

            if (score != null)
            {
                return "bronze";
            }

            return "none";
        }

        public async Task<long> GetUserPosition(string userId)
        {
            var score = await _leaderboard.SortedSetRankAsync("GOLD_LEAGUE", userId.ToString(), Order.Descending);

            if (score.HasValue)
            {
                return score.Value;
            }

            score = await _leaderboard.SortedSetRankAsync("SILVER_LEAGUE", userId.ToString(), Order.Descending);

            if (score.HasValue)
            {
                return score.Value;
            }

            score = await _leaderboard.SortedSetRankAsync("BRONZE_LEAGUE", userId.ToString(), Order.Descending);

            if (score.HasValue)
            {
                return score.Value;
            }


            return 0;
        }

        public async Task UpdateGoldLeague(string userId, ulong weekExp)
        {
            await _leaderboard.SortedSetRemoveAsync("GOLD_LEAGUE", userId.ToString());
            await _leaderboard.SortedSetAddAsync("GOLD_LEAGUE", userId.ToString(), weekExp);
        }

        public async Task UpdateSilverLeague(string userId, ulong weekExp)
        {
            await _leaderboard.SortedSetRemoveAsync("SILVER_LEAGUE", userId.ToString());
            await _leaderboard.SortedSetAddAsync("SILVER_LEAGUE", userId.ToString(), weekExp);
        }

        public async Task UpdateBronzeLeague(string userId, ulong weekExp)
        {
            await _leaderboard.SortedSetRemoveAsync("BRONZE_LEAGUE", userId.ToString());
            await _leaderboard.SortedSetAddAsync("BRONZE_LEAGUE", userId.ToString(), weekExp);
        }

        public async Task Update(string userId, ulong weekExp)
        {
            string league = await GetUserLeague(userId);
            Console.WriteLine("---> LEADERBOARD NEW ENTRY: " + userId + ", EXP: " + weekExp);

            switch (league)
            {
                case "gold":
                    await UpdateGoldLeague(userId, weekExp);
                    break;
                case "silver":
                    await UpdateSilverLeague(userId, weekExp);
                    break;
                default:
                    await UpdateBronzeLeague(userId, weekExp);
                    break;
            }
        }

        public async Task Promote(string userId)
        {
            
            string league = await GetUserLeague(userId);

            var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId.ToString() == userId).FirstOrDefault();

            switch (league)
            {
                case "gold":
                    await _leaderboard.SortedSetRemoveAsync("GOLD_LEAGUE", userId.ToString());
                    await UpdateBronzeLeague(userId, 0);

                    if (userStats != null)
                    {
                        userStats.UserExp += 2000;
                    }

                    break;
                case "silver":
                    await _leaderboard.SortedSetRemoveAsync("SILVER_LEAGUE", userId.ToString());
                    await UpdateGoldLeague(userId, 0);

                    if (userStats != null)
                    {
                        userStats.UserExp += 1000;
                    }

                    break;
                case "bronze":
                    await _leaderboard.SortedSetRemoveAsync("BRONZE_LEAGUE", userId.ToString());
                    await UpdateSilverLeague(userId, 0);

                    if (userStats != null)
                    {
                        userStats.UserExp += 500;
                    }

                    break;
            }
            await _academaticaDbContext.SaveChangesAsync();
        }

        public async Task Demote(string userId)
        {
            string league = await GetUserLeague(userId);

            switch (league)
            {
                case "gold":
                    await _leaderboard.SortedSetRemoveAsync("GOLD_LEAGUE", userId.ToString());
                    await UpdateSilverLeague(userId, 0);
                    break;
                case "silver":
                    await _leaderboard.SortedSetRemoveAsync("SILVER_LEAGUE", userId.ToString());
                    await UpdateBronzeLeague(userId, 0);
                    break;
                case "bronze":
                    await _leaderboard.SortedSetRemoveAsync("BRONZE_LEAGUE", userId.ToString());
                    break;
            }
        }

        public async Task<SortedSetEntry[]> GetGoldLeague(int page)
        {
            int start = 20 * (page - 1);
            int end = (20 * page) - 1;

            return await _leaderboard.SortedSetRangeByRankWithScoresAsync("GOLD_LEAGUE", start, end, Order.Descending);
        }

        public async Task<SortedSetEntry[]> GetSilverLeague(int page)
        {
            int start = 20 * (page - 1);
            int end = (20 * page) - 1;

            return await _leaderboard.SortedSetRangeByRankWithScoresAsync("SILVER_LEAGUE", start, end, Order.Descending);
        }

        public async Task<SortedSetEntry[]> GetBronzeLeague(int page)
        {
            int start = 20 * (page - 1);
            int end = (20 * page) - 1;

            return await _leaderboard.SortedSetRangeByRankWithScoresAsync("BRONZE_LEAGUE", start, end, Order.Descending);
        }

        public async Task<long> GetGoldLeagueCount()
        {
            var count = await _leaderboard.SortedSetLengthAsync("GOLD_LEAGUE");

            return count / 20 + (count % 20 > 0 ? 1 : 0);
        }

        public async Task<long> GetSilverLeagueCount()
        {
            var count = await _leaderboard.SortedSetLengthAsync("SILVER_LEAGUE");

            return count / 20 + (count % 20 > 0 ? 1 : 0);
        }

        public async Task<long> GetBronzeLeagueCount()
        {
            var count = await _leaderboard.SortedSetLengthAsync("BRONZE_LEAGUE");

            return count / 20 + (count % 20 > 0 ? 1 : 0);
        }
    }
}
