using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.DTOs
{
    public class GetUserStatsResponseDto
    {
        public uint BuoysLeft { get; set; }
        public ulong DaysStreak { get; set; }
    }
}
