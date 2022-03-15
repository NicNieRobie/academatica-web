using Academatica.Api.Common.Models;
using Academatica.Api.Course.DTOs;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.Services.Grpc
{
    public class PracticeAchievementsDataClient : IPracticeAchievementsDataClient
    {
        private readonly IConfiguration _configuration;

        public PracticeAchievementsDataClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<AchievementDto> GetPracticeAchievements(Guid userId, string classId, int mistakeCount)
        {
            Console.WriteLine($"--> Calling GRPC Service {_configuration["GrpcAchievementsService"]}");
            var channel = GrpcChannel.ForAddress(_configuration["GrpcAchievementsService"]);
            var client = new GrpcAchievements.GrpcAchievementsClient(channel);
            var request = new GetPracticeAchievementsRequest()
            {
                ClassId = classId,
                MistakesMade = mistakeCount,
                UserId = userId.ToString()
            };

            try
            {
                var reply = client.GetPracticeAchievements(request);
                return reply.Achievements.Select(x => new AchievementDto()
                {
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not call GRPC Server {ex.Message}");
                return null;
            }
        }
    }
}
