using Academatica.Api.Common.Models;
using System;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services {
    public interface IConfirmationCodeManager {
        Task<string> CreateEmailConfirmationCode(Guid userId);
        Task<string> GetEmailConfirmationCode(Guid userId);
        Task<string> CreatePasswordConfirmationCode(Guid userId);
        Task<string> GetPasswordConfirmationCode(Guid userId);
        Task RemoveEmailConfirmationCode(Guid userId);
        Task RemovePasswordConfirmationCode(Guid userId);
    }
}