using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Course.DTOs
{
    public class FinishClassResponseDto
    {
        public int Exp { get; set; }
        public IEnumerable<AchievementDto> Achievements { get; set; }
    }
}
