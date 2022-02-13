using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public class AchievementsManager : IAchievementsManager
    {
        private AcadematicaDbContext _academaticaDbContext;

        public AchievementsManager(AcadematicaDbContext academaticaDbContext)
        {
            _academaticaDbContext = academaticaDbContext;
        }

        private async Task AddAchievement(User user, Achievement achievement)
        {
            if (user == null || achievement == null)
            {
                return;
            }

            var userAchievements = _academaticaDbContext.UserAchievements.Where(x => x.UserId == user.Id).ToList();
            var userAchievementIds = userAchievements.Select(x => x.AchievementId);

            if (!userAchievementIds.Contains(achievement.Id))
            {
                _academaticaDbContext.UserAchievements.Add(new UserAchievement
                {
                    User = user,
                    UserId = user.Id,
                    Achievement = achievement,
                    AchievementId = achievement.Id,
                    AchievedAt = DateTime.UtcNow,
                    AchievedAmount = 1
                });
            } else
            {
                var entry = userAchievements.Where(x => x.AchievementId == achievement.Id).FirstOrDefault();

                if (entry != null)
                {
                    entry.AchievedAmount++;
                    entry.AchievedAt = DateTime.UtcNow;
                }
            }

            await _academaticaDbContext.SaveChangesAsync();
        }

        private Achievement FindAchievementByName(string name)
        {
            return _academaticaDbContext.Achievements.Where(x => x.Name == name).FirstOrDefault();
        }

        private async Task AddAchievementByName(User user, string name)
        {
            var achievement = FindAchievementByName(name);
            await AddAchievement(user, achievement);
        }

        private async Task<bool> AddAchievementIfNotPresentByName(User user, string name)
        {
            var achievement = FindAchievementByName(name);
            var userAchievementIds = _academaticaDbContext.UserAchievements.Where(x => x.UserId == user.Id).Select(x => x.AchievementId).ToList();

            if (achievement == null)
            {
                return false;
            }

            if (!userAchievementIds.Contains(achievement.Id))
            {
                await AddAchievement(user, achievement);
                return true;
            }

            return false;
        }

        private async Task<bool> AddAchievementOncePerDayByName(User user, string name)
        {
            var achievement = FindAchievementByName(name);
            var userAchievementTodayIds = _academaticaDbContext.UserAchievements
                .Where(x => x.UserId == user.Id && x.AchievedAt == DateTime.Today.Date).Select(x => x.AchievementId).ToList();

            if (achievement == null)
            {
                return false;
            }

            if (!userAchievementTodayIds.Contains(achievement.Id))
            {
                await AddAchievement(user, achievement);
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Achievement>> CheckForNewAchievements(Guid userId, Class finishedClass, int mistakesMade)
        {
            var user = _academaticaDbContext.Users.Where(x => x.Id == userId).FirstOrDefault();

            if (user == null)
            {
                return new List<Achievement>();
            }

            var finishedClasses = _academaticaDbContext.UserClasses.Where(x => x.UserId == userId).ToList();
            var finishedTopics = _academaticaDbContext.UserTopic.Where(x => x.UserId == userId).ToList();
            var finishedTiers = _academaticaDbContext.UserTier.Where(x => x.UserId == userId).ToList();

            var newAchievements = new List<Achievement>();

            if (finishedClasses.Count() == 1)
            {
                var isAdded = await AddAchievementIfNotPresentByName(user, "First steps");
                if (isAdded)
                {
                    newAchievements.Add(FindAchievementByName("First steps"));
                }
            }

            if (finishedTopics.Count() == 1)
            {
                var isAdded = await AddAchievementIfNotPresentByName(user, "Pushing forward");
                if (isAdded)
                {
                    newAchievements.Add(FindAchievementByName("Pushing forward"));
                }
            }

            if (finishedTiers.Count() == 1)
            {
                var isAdded = await AddAchievementIfNotPresentByName(user, "Raising the bar");
                if (isAdded)
                {
                    newAchievements.Add(FindAchievementByName("Raising the bar"));
                }
            }

            if (mistakesMade == 0)
            {

                await AddAchievementByName(user, "Flawless run");
                newAchievements.Add(FindAchievementByName("Flawless run"));
            }

            if (finishedClass != null)
            {
                if (finishedClasses.Where(x => x.CompletedAt.Date == DateTime.Today.Date).Count() == 3)
                {
                    var isAdded = await AddAchievementOncePerDayByName(user, "Triple strike");
                    if (isAdded)
                    {
                        newAchievements.Add(FindAchievementByName("Triple strike"));
                    }
                }

                var topicClassesIds = _academaticaDbContext.Classes.Where(x => x.TopicId == finishedClass.TopicId).Select(x => x.Id).ToList();
                var completedTopicClassesIds = _academaticaDbContext.UserClasses
                    .Where(x => topicClassesIds.Contains(x.ClassId) && x.CompletedAt.Date == DateTime.Today.Date).Select(x => x.ClassId).ToList();

                if (finishedTopics.Select(x => x.TopicId).Contains(finishedClass.TopicId) && Enumerable.SequenceEqual(topicClassesIds, completedTopicClassesIds))
                {
                    var isAdded = await AddAchievementOncePerDayByName(user, "Per aspera ad astra");
                    if (isAdded)
                    {
                        newAchievements.Add(FindAchievementByName("Per aspera ad astra"));
                    }
                }
            }

            return newAchievements;
        }

        public IEnumerable<AchievementData> GetUserAchievements(Guid userId)
        {
            var user = _academaticaDbContext.Users.Where(x => x.Id == userId).FirstOrDefault();

            if (user == null)
            {
                return new List<AchievementData>();
            }

            return _academaticaDbContext.Achievements.Join(
                _academaticaDbContext.UserAchievements.Where(x => x.UserId == userId),
                a => a.Id,
                ua => ua.AchievementId,
                (a, ua) => new AchievementData()
                {
                    Name = a.Name,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    AchievedAmount = ua.AchievedAmount
                });
        }
    }
}
