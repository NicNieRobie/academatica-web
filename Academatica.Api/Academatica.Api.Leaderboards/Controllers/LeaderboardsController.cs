using Academatica.Api.Common.Data;
using Academatica.Api.Leaderboards.DTOs;
using Academatica.Api.Leaderboards.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/leaderboard")]
    [ApiController]
    public class LeaderboardsController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly AcadematicaDbContext _academaticaDbContext;

        public LeaderboardsController(ILeaderboardService leaderboardService, AcadematicaDbContext academaticaDbContext)
        {
            _leaderboardService = leaderboardService;
            _academaticaDbContext = academaticaDbContext;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserLeaderboardStats([FromRoute] Guid id)
        {
            string league = await _leaderboardService.GetUserLeague(id.ToString());
            long position = await _leaderboardService.GetUserPosition(id.ToString());

            GetUserLeaderboardStatsReponseDto leaderboardStats = new GetUserLeaderboardStatsReponseDto()
            {
                League = league,
                Rank = position
            };

            return Ok(leaderboardStats);
        }

        [HttpGet]
        [Route("gold")]
        public async Task<IActionResult> GetGoldenLeagueList([FromQuery] int page)
        {
            if (page <= 0 || page > await _leaderboardService.GetGoldLeagueCount())
            {
                return BadRequest("Invalid page number.");
            }

            var leaderboardEntries = await _leaderboardService.GetGoldLeague(page);

            List<LeaderboardEntryDto> leaderboard = new List<LeaderboardEntryDto>();

            foreach (var entry in leaderboardEntries)
            {
                var user = _academaticaDbContext.Users.Where(x => x.Id.ToString() == entry.Element.ToString()).FirstOrDefault();

                if (user == null)
                {
                    continue;
                }

                var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId.ToString() == entry.Element.ToString()).FirstOrDefault();

                if (userStats == null)
                {
                    continue;
                }

                LeaderboardEntryDto leaderboardEntryDto = new LeaderboardEntryDto()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ExpThisWeek = Convert.ToUInt64(entry.Score),
                    Rank = await _leaderboardService.GetUserPosition(user.Id.ToString()) + 1,
                    Username = user.UserName
                };

                leaderboard.Add(leaderboardEntryDto);
            }

            return Ok(new GetLeaguePageResponseDto()
            {
                Leaderboard = leaderboard
            });
        }

        [HttpGet]
        [Route("silver")]
        public async Task<IActionResult> GetSilverLeagueList([FromQuery] int page)
        {
            if (page <= 0 || page > await _leaderboardService.GetSilverLeagueCount())
            {
                return BadRequest("Invalid page number.");
            }

            var leaderboardEntries = await _leaderboardService.GetSilverLeague(page);

            List<LeaderboardEntryDto> leaderboard = new List<LeaderboardEntryDto>();

            foreach (var entry in leaderboardEntries)
            {
                var user = _academaticaDbContext.Users.Where(x => x.Id.ToString() == entry.Element.ToString()).FirstOrDefault();

                if (user == null)
                {
                    continue;
                }

                var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId.ToString() == entry.Element.ToString()).FirstOrDefault();

                if (userStats == null)
                {
                    continue;
                }

                LeaderboardEntryDto leaderboardEntryDto = new LeaderboardEntryDto()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ExpThisWeek = Convert.ToUInt64(entry.Score),
                    Rank = await _leaderboardService.GetUserPosition(user.Id.ToString()) + 1,
                    Username = user.UserName
                };

                leaderboard.Add(leaderboardEntryDto);
            }

            return Ok(new GetLeaguePageResponseDto()
            {
                Leaderboard = leaderboard
            });
        }

        [HttpGet]
        [Route("bronze")]
        public async Task<IActionResult> GetBronzeLeagueList([FromQuery] int page)
        {
            if (page <= 0 || page > await _leaderboardService.GetBronzeLeagueCount())
            {
                return BadRequest("Invalid page number.");
            }

            var leaderboardEntries = await _leaderboardService.GetBronzeLeague(page);

            List<LeaderboardEntryDto> leaderboard = new List<LeaderboardEntryDto>();

            foreach (var entry in leaderboardEntries)
            {
                var user = _academaticaDbContext.Users.Where(x => x.Id.ToString() == entry.Element.ToString()).FirstOrDefault();

                if (user == null)
                {
                    continue;
                }

                var userStats = _academaticaDbContext.UserStats.Where(x => x.UserId.ToString() == entry.Element.ToString()).FirstOrDefault();

                if (userStats == null)
                {
                    continue;
                }

                LeaderboardEntryDto leaderboardEntryDto = new LeaderboardEntryDto()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ExpThisWeek = Convert.ToUInt64(entry.Score),
                    Rank = await _leaderboardService.GetUserPosition(user.Id.ToString()) + 1,
                    Username = user.UserName
                };

                leaderboard.Add(leaderboardEntryDto);
            }

            return Ok(new GetLeaguePageResponseDto()
            {
                Leaderboard = leaderboard
            });
        }
    }
}
