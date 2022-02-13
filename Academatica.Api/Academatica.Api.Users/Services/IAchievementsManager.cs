using Academatica.Api.Common.Models;
using Academatica.Api.Users.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public interface IAchievementsManager
    {
        public Task<IEnumerable<Achievement>> CheckForNewAchievements(Guid userId, Class finishedClass, int mistakesMade);
        public IEnumerable<AchievementData> GetUserAchievements(Guid userId);
    }
}
