using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMath.Api.Common.Data;
using SmartMath.Api.Common.Models;
using SmartMath.Api.Users.DBOs;
using SmartMath.Api.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Users.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly SmartMathDbContext _smartMathDbContext;

        public UsersController(SmartMathDbContext smartMathDbContext)
        {
            _smartMathDbContext = smartMathDbContext;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUser([FromQuery(Name = "id")] Guid userId)
        {
            var foundUser = await _smartMathDbContext.Users.FindAsync(userId);

            if (foundUser == null)
            {
                return BadRequest(new GetUserResponseDbo()
                {
                    Success = false,
                    Error = "Invalid user ID"
                });
            }

            var userStats = _smartMathDbContext.UserStats.FirstOrDefault(x => x.UserId == foundUser.Id);

            var response = new GetUserResponseDbo()
            {
                Success = true,
                Username = foundUser.UserName,
                FirstName = foundUser.FirstName,
                LastName = foundUser.LastName,
                ProfilePicURL = foundUser.ProfilePicUrl,
                DaysStreak = userStats.DaysStreak,
                Exp = userStats.UserExp,
                BuoysLeft = userStats.BuoysLeft
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("achievements")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserAchievements([FromQuery(Name = "id")] Guid userId)
        {
            var foundUser = await _smartMathDbContext.Users.FindAsync(userId);

            if (foundUser == null)
            {
                return BadRequest(new GetUserAchievementsResponseDbo()
                {
                    Success = false,
                    Error = "Invalid user ID"
                });
            }

            var userAchievementEntries = _smartMathDbContext.UserAchievements.Where(x => x.UserId == foundUser.Id).ToList();

            List<ReceivedAchievementEntry> userAchievements = new List<ReceivedAchievementEntry>();

            foreach (var entry in userAchievementEntries)
            {
                var foundAchievement = await _smartMathDbContext.Achievements.FindAsync(entry.AchievementId);

                if (foundAchievement != null)
                {
                    userAchievements.Add(new ReceivedAchievementEntry
                    {
                        Id = foundAchievement.Id,
                        Name = foundAchievement.Name,
                        Description = foundAchievement.Description,
                        ImageUrl = foundAchievement.ImageUrl,
                        AchievedAt = entry.AchievedAt
                    });
                }
            }

            var response = new GetUserAchievementsResponseDbo()
            {
                Success = true,
                Achievements = userAchievements
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("course/tiers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserTierStats([FromQuery(Name = "userId")] Guid userId)
        {
            var foundUser = await _smartMathDbContext.Users.FindAsync(userId);

            if (foundUser == null)
            {
                return BadRequest(new GetUserAchievementsResponseDbo()
                {
                    Success = false,
                    Error = "Invalid user ID"
                });
            }

            var userTiersIds = _smartMathDbContext.UserTier.Where(x => x.UserId == userId).Select(x => x.TierId).ToList();

            List<UserStatsTier> userTiers = new List<UserStatsTier>();

            var tiers = _smartMathDbContext.Tiers.ToList();

            foreach (var entry in tiers)
            {
                var tierTopics = _smartMathDbContext.UserTopic.Join(
                    _smartMathDbContext.Topics,
                    ut => ut.TopicId,
                    t => t.Id,
                    (ut, t) => new
                    {
                        Id = t.Id,
                        TierId = t.TierId,
                        UserId = ut.UserId
                    }).ToList();

                double finishedTopicsCount = tierTopics.Where(x => x.UserId == userId && x.TierId == entry.Id).Count();
                var topicsCount = _smartMathDbContext.Topics.Where(x => x.TierId == entry.Id).Count();

                userTiers.Add(new UserStatsTier
                {
                    Id = entry.Id,
                    Name = entry.Name,
                    Description = entry.Description,
                    ImageUrl = entry.ImageUrl,
                    IsFinished = userTiersIds.Contains(entry.Id),
                    FinishedPercentage = Convert.ToInt32(finishedTopicsCount / topicsCount * 100)
                });
            }

            var response = new GetUserTiersResponseDbo()
            {
                Success = true,
                TierStats = userTiers
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("course/topics")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserTierTopicsStats(
            [FromQuery(Name = "userId")] Guid userId, 
            [FromQuery(Name = "tierId")] Guid tierId, 
            [FromQuery(Name = "isAlgebra")] bool isAlgebra)
        {
            var foundUser = await _smartMathDbContext.Users.FindAsync(userId);

            if (foundUser == null)
            {
                return BadRequest(new GetUserTierTopicsResponseDbo()
                {
                    Success = false,
                    Error = "Invalid user ID"
                });
            }

            var userTopicIds = _smartMathDbContext.UserTopic.Where(x => x.UserId == foundUser.Id).Select(x => x.TopicId).ToList();

            List<UserStatsTierTopic> userTopics = new List<UserStatsTierTopic>();

            foreach (var entry in _smartMathDbContext.Topics.Where(x => x.TierId == tierId && x.IsAlgebraTopic == isAlgebra).ToList())
            {
                userTopics.Add(new UserStatsTierTopic
                {
                    Id = entry.Id,
                    Description = entry.Description,
                    Name = entry.Name,
                    ImageUrl = entry.ImageUrl,
                    IsAlgebraTopic = entry.IsAlgebraTopic,
                    IsFinished = userTopicIds.Contains(entry.Id)
                });
            }

            var response = new GetUserTierTopicsResponseDbo()
            {
                Success = true,
                TopicStats = userTopics
            };

            return Ok(response);
        }

        /*[HttpPost]
        [Route("achievements/add")]
        public async Task<IActionResult> AddAchievement(AddAchievementDbo addAchievementDbo)
        {
            var achievement = _smartMathDbContext.Achievements.Add(new Achievement
            {
                Id = new Guid(),
                Name = addAchievementDbo.Name,
                Description = addAchievementDbo.Description,
                ImageUrl = addAchievementDbo.ImageUrl
            });

            await _smartMathDbContext.SaveChangesAsync();

            return Ok(achievement.Entity);
        }

        [HttpPost]
        [Route("achievements")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddUserAchievement([FromQuery(Name = "userId")] Guid userId, [FromQuery(Name = "achievementId")] Guid achievementId)
        {
            var foundUser = await _smartMathDbContext.Users.FindAsync(userId);

            if (foundUser == null)
            {
                return BadRequest();
            }

            var foundAchievement = await _smartMathDbContext.Achievements.FindAsync(achievementId);

            _smartMathDbContext.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementId = achievementId,
                User = foundUser,
                Achievement = foundAchievement,
                AchievedAt = DateTime.UtcNow
            });

            await _smartMathDbContext.SaveChangesAsync();

            return NoContent();
        }*/
    }
}
