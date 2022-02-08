using Academatica.Api.Common.Models;
using System;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services {
    public interface IConfirmationCodeManager {
        Task<string> CreateConfirmationCode(Guid userId);
        Task<string> GetConfirmationCode(Guid userId);
        Task RemoveConfirmationCode(Guid userId);
    }
}