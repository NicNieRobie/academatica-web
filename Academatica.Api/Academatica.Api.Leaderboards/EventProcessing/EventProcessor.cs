using Academatica.Api.Common.Data;
using Academatica.Api.Leaderboards.DTOs;
using Academatica.Api.Leaderboards.Services;
using Academatica.Api.Users.DTOs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.EventProcessing
{
    enum EventType
    {
        ExpChangePublished,
        WeeklyUpdatePublished,
        Unknown
    }

    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task ProcessEvent(string message)
        {
            EventType eventType = DetermineEventType(message);

            switch (eventType)
            {
                case EventType.ExpChangePublished:
                    await RegisterExpChange(message);
                    break;
                case EventType.WeeklyUpdatePublished:
                    await WeeklyChange();
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEventType(string eventMessage)
        {
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(eventMessage);

            switch (eventType.Event)
            {
                case "EXP_CHANGE":
                    return EventType.ExpChangePublished;
                case "WEEKLY_UPDATE":
                    return EventType.WeeklyUpdatePublished;
                default:
                    return EventType.Unknown;
            }
        }

        private async Task RegisterExpChange(string message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var leaderboardService = scope.ServiceProvider.GetRequiredService<ILeaderboardService>();

                var expChangeDto = JsonSerializer.Deserialize<ExpChangePublishDto>(message);

                await leaderboardService.Update(expChangeDto.UserId.ToString(), expChangeDto.ExpThisWeek);
            }
        }

        private async Task WeeklyChange()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var leaderboardManager = scope.ServiceProvider.GetRequiredService<ILeaderboardManager>();

                await leaderboardManager.UpdateLeaderboards();
            }
        }
    }
}
