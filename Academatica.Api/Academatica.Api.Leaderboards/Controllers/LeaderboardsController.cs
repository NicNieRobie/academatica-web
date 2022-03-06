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
    /// <summary>
    /// Controller responsible for handling requests related to the Academatica leaderboards system.
    /// </summary>
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

        /// <summary>
        /// Endpoint used to receive information about the user's standing in the leaderboard.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>User league and position.</returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserLeaderboardStats([FromRoute] Guid id)
        {
            string league = await _leaderboardService.GetUserLeague(id.ToString());
            long position = await _leaderboardService.GetUserPosition(id.ToString());

            GetUserLeaderboardStatsReponseDto leaderboardStats = new GetUserLeaderboardStatsReponseDto()
            {
                League = league,
                Rank = position + 1
            };

            return Ok(leaderboardStats);
        }

        /// <summary>
        /// Endpoint used to receive a specified page of the golden league leaderboard.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <returns>Leaderboard segment.</returns>
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

        /// <summary>
        /// Endpoint used to receive a specified page of the silver league leaderboard.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <returns>Leaderboard segment.</returns>
        [HttpGet]
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

        /// <summary>
        /// Endpoint used to receive a specified page of the bronze league leaderboard.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <returns>Leaderboard segment.</returns>
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
