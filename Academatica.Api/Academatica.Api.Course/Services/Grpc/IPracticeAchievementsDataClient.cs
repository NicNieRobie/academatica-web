using Academatica.Api.Common.Models;
using Academatica.Api.Course.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.Services.Grpc
{
    public interface IPracticeAchievementsDataClient
    {
        IEnumerable<AchievementDto> GetPracticeAchievements(Guid userId, string classId, int mistakeCount);
    }
}
