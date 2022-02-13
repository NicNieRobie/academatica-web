using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class GetUserAchievementsResponseDto
    {
        public IEnumerable<AchievementDto> Achievements { get; set; }
    }
}
