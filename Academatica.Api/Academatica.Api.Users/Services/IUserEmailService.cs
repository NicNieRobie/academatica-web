using Academatica.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public interface IUserEmailService
    {
        Task SendConfirmationCode(User user, string code);
        Task SendEmailChangeNotification(User user, string oldEmail, string newEmail, string rollbackUrl);
        Task SendNewEmailConfirmation(User user, string newEmail, string callbackUrl);
    }
}
