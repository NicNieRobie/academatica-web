using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public class UserStatsManager : IUserStatsManager
    {
        private readonly AcadematicaDbContext _academaticaDbContext;

        public UserStatsManager(AcadematicaDbContext academaticaDbContext)
        {
            _academaticaDbContext = academaticaDbContext;
        }

        public async Task UpdateUsersBuoys()
        {
            var statsEntry = _academaticaDbContext.UserStats;

            foreach (var entry in statsEntry)
            {
                if (entry.BuoysLeft < 5)
                {
                    entry.BuoysLeft++;
                }
            }

            await _academaticaDbContext.SaveChangesAsync();
        }

        public async Task UpdateUsersDayStreaks()
        {
            var statsEntry = _academaticaDbContext.UserStats;
            var userClasses = _academaticaDbContext.UserClasses;

            foreach (var entry in statsEntry)
            {
                var didFinishClassesYesterday = _academaticaDbContext.UserClasses
                    .Where(x => x.UserId == entry.UserId && x.CompletedAt.Date == DateTime.Today.AddDays(-1).Date)
                    .Count() > 0;

                if (didFinishClassesYesterday)
                {
                    entry.DaysStreak += 1;
                } else
                {
                    entry.DaysStreak = 0;
                }
            }

            await _academaticaDbContext.SaveChangesAsync();
        }

        public async Task UpdateUsersExpThisWeek()
        {
            var statsEntry = _academaticaDbContext.UserStats;

            foreach (var entry in statsEntry)
            {
                entry.UserExpThisWeek = 0;
            }

            await _academaticaDbContext.SaveChangesAsync();
        }
    }
}
