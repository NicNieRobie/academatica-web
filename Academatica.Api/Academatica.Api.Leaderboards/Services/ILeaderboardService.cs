using StackExchange.Redis;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.Services
{
    public interface ILeaderboardService
    {
        Task<string> GetUserLeague(string userId);
        Task<long> GetUserPosition(string userId);
        Task<SortedSetEntry[]> GetGoldLeague(int page);
        Task<SortedSetEntry[]> GetSilverLeague(int page);
        Task<SortedSetEntry[]> GetBronzeLeague(int page);
        Task UpdateGoldLeague(string userId, ulong weekExp);
        Task UpdateSilverLeague(string userId, ulong weekExp);
        Task UpdateBronzeLeague(string userId, ulong weekExp);
        Task Update(string userId, ulong weekExp);
        Task Promote(string userId);
        Task Demote(string userId);
        Task<long> GetGoldLeagueCount();
        Task<long> GetSilverLeagueCount();
        Task<long> GetBronzeLeagueCount();
    }
}
