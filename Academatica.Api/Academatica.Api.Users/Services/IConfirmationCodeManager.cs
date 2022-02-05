using System;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services {
    public interface IConfirmationCodeManager {
        Task CreateConfirmationCode(Guid userId);
        Task<string> GetConfirmationCode(Guid userId);
    }
}