using Academatica.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public interface IUserStatsManager
    {
        Task UpdateUsersBuoys();
        Task UpdateUsersDayStreaks();
        Task UpdateUsersExpThisWeek();
    }
}
