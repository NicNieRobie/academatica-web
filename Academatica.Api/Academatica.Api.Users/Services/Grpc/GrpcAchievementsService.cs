using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services.Grpc
{
    public class GrpcAchievementsService : GrpcAchievements.GrpcAchievementsBase
    {
        private readonly AcadematicaDbContext _academaticaDbContext;
        private readonly IAchievementsManager _achievementsManager;

        public GrpcAchievementsService(AcadematicaDbContext academaticaDbContext, IAchievementsManager achievementsManager)
        {
            _academaticaDbContext = academaticaDbContext;
            _achievementsManager = achievementsManager;
        }

        public override async Task<PracticeAchievementsResponse> GetPracticeAchievements(GetPracticeAchievementsRequest request, ServerCallContext context)
        {
            var response = new PracticeAchievementsResponse();

            var user = _academaticaDbContext.Users.Where(x => x.Id.ToString() == request.UserId).FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine(" -> GRPC ERROR: User was null.");
                return response;
            }

            IEnumerable<Achievement> receivedAchievements = new List<Achievement>();
            if (request.ClassId != null)
            {
                var finishedClass = _academaticaDbContext.Classes.Where(x => x.Id == request.ClassId).FirstOrDefault();
                receivedAchievements = await _achievementsManager.CheckForNewAchievements(user.Id, finishedClass, request.MistakesMade);
            } else
            {
                receivedAchievements = await _achievementsManager.CheckForNewAchievements(user.Id, null, request.MistakesMade);
            }

            response.Achievements.AddRange(receivedAchievements.Select(x => new GrpcAchievementModel()
            {
                Id = x.Id.ToString(),
                Description = x.Description,
                Name = x.Name,
                ImageUrl = x.ImageUrl
            }));

            return response;
        }
    }
}
