using Academatica.Api.Common.Configuration;
using Academatica.Api.Common.Models;
using Academatica.Api.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services
{
    public class UserEmailService : EmailService, IUserEmailService
    {
        private readonly IWebHostEnvironment _env;

        public UserEmailService(IOptions<MailConfig> options, IWebHostEnvironment env) : base(options)
        {
            _env = env;
        }

        public async Task SendConfirmationCode(User user, string code, NotificationType notifType)
        {
            var fileName = "";

            switch (notifType)
            {
                case NotificationType.EmailChangeNotification:
                    fileName = "EMailChangeCodeTemplate.html";
                    break;
                case NotificationType.PasswordChangeNotification:
                    fileName = "PasswordChangeCodeTemplate.html";
                    break;
            }

            var pathToFile = _env.WebRootPath
                    + Path.DirectorySeparatorChar.ToString()
                    + "resources"
                    + Path.DirectorySeparatorChar.ToString()
                    + "templates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "EmailTemplates"
                    + Path.DirectorySeparatorChar.ToString()
                    + fileName;

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = File.OpenText(pathToFile))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            var messageBody = string.Format(builder.HtmlBody, user.UserName, code);

            await SendEmailAsync(user.Email, $"Ваш код Academatica - {code}", messageBody);
        }

        public async Task SendEmailChangeNotification(User user, string oldEmail, string newEmail, string rollbackUrl)
        {
            var pathToFile = _env.WebRootPath
                    + Path.DirectorySeparatorChar.ToString()
                    + "resources"
                    + Path.DirectorySeparatorChar.ToString()
                    + "templates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "EmailTemplates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "EMailChangeNotificationTemplate.html";

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = File.OpenText(pathToFile))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            var messageBody = string.Format(builder.HtmlBody, user.UserName, newEmail, rollbackUrl);

            await SendEmailAsync(oldEmail, $"Ваша учётная запись Academatica изменена", messageBody);
        }

        public async Task SendPasswordChangeNotification(User user)
        {
            var pathToFile = _env.WebRootPath
                    + Path.DirectorySeparatorChar.ToString()
                    + "resources"
                    + Path.DirectorySeparatorChar.ToString()
                    + "templates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "EmailTemplates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "PasswordChangeNotificationTemplate.html";

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = File.OpenText(pathToFile))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            var messageBody = string.Format(builder.HtmlBody, user.UserName);

            await SendEmailAsync(user.Email, $"Ваша учётная запись Academatica изменена", messageBody);
        }

        public async Task SendNewEmailConfirmation(User user, string newEmail, string callbackUrl)
        {
            var pathToFile = _env.WebRootPath
                    + Path.DirectorySeparatorChar.ToString()
                    + "resources"
                    + Path.DirectorySeparatorChar.ToString()
                    + "templates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "EmailTemplates"
                    + Path.DirectorySeparatorChar.ToString()
                    + "EMailChangeTemplate.html";

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = File.OpenText(pathToFile))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            var messageBody = string.Format(builder.HtmlBody, user.UserName, callbackUrl);

            await SendEmailAsync(newEmail, "Подтверждение нового адреса учётной записи Academatica", messageBody);
        }
    }
}
