using Academatica.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public enum NotificationType
    {
        EmailChangeNotification,
        PasswordChangeNotification
    }

    public interface IUserEmailService
    {
        Task SendConfirmationCode(User user, string code, NotificationType notifType);
        Task SendEmailChangeNotification(User user, string oldEmail, string newEmail, string rollbackUrl);
        Task SendPasswordChangeNotification(User user);
        Task SendNewEmailConfirmation(User user, string newEmail, string callbackUrl);
    }
}
