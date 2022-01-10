using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartMath.Api.Auth.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailConfig _mailConfig;

        public EmailService(IOptions<MailConfig> options)
        {
            _mailConfig = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var fromMail = "academatica@yandex.ru";
            var passcode = _mailConfig.Passcode;

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Academatica", "academatica@yandex.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.ru", 465, true);
                await client.AuthenticateAsync(fromMail, passcode);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
