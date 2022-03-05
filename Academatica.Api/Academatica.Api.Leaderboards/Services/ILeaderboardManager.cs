using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Leaderboards.Services
{
    interface ILeaderboardManager
    {
        Task UpdateLeaderboards();
    }
}
