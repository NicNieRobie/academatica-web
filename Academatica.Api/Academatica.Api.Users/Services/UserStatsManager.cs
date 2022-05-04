using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.DTOs;
using Academatica.Api.Users.Services.RabbitMQ;
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
        private readonly IMessageBusClient _messageBusClient;

        public UserStatsManager(AcadematicaDbContext academaticaDbContext, IMessageBusClient messageBusClient)
        {
            _academaticaDbContext = academaticaDbContext;
            _messageBusClient = messageBusClient;
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
            var userClasses = _academaticaDbContext.UserClasses.AsEnumerable().ToList();
            var statsEntry = _academaticaDbContext.UserStats.AsEnumerable();

            foreach (var entry in statsEntry)
            {
                var didFinishClassesYesterday = userClasses
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
            var statsEntry = _academaticaDbContext.UserStats.AsEnumerable();

            foreach (var entry in statsEntry)
            {
                entry.UserExpThisWeek = 0;
            }

            WeeklyUpdatePublishDto updatePublishDto = new WeeklyUpdatePublishDto()
            {
                Event = "WEEKLY_UPDATE"
            };

            _messageBusClient.PublishWeeklyUpdate(updatePublishDto);

            await _academaticaDbContext.SaveChangesAsync();
        }
    }
}
