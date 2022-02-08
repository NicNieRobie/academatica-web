using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Distributed;
using MimeKit;

namespace Academatica.Api.Users.Services {

    public class ConfirmationCodeManager: IConfirmationCodeManager {
        private readonly IDistributedCache _codeCache;
        private readonly IEmailSender _emailService;
        private readonly IWebHostEnvironment _env;

        public ConfirmationCodeManager(IDistributedCache codeCache) {
            _codeCache = codeCache;
        }

        public async Task<string> GetConfirmationCode(Guid userId) {
            return await _codeCache.GetRecordAsync<string>($"code_{userId}");
        }

        public async Task<string> CreateConfirmationCode(Guid userId)
        {
            Random r = new Random();
            var code = r.Next(100000, 1000000);
            await _codeCache.SetRecordAsync($"code_{userId}", code.ToString());
            return code.ToString();
        }

        public async Task RemoveConfirmationCode(Guid userId)
        {
            await _codeCache.RemoveAsync($"code_{userId}");
        }
    }
}