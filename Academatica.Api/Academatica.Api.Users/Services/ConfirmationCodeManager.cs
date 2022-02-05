using System;
using System.Threading.Tasks;
using Academatica.Api.Users.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace Academatica.Api.Users.Services {

    public class ConfirmationCodeManager: IConfirmationCodeManager {
        private readonly IDistributedCache _codeCache;

        public ConfirmationCodeManager(IDistributedCache codeCache) {
            _codeCache = codeCache;
        }

        public async Task CreateConfirmationCode(Guid userId) {
            Random r = new Random();
            var code = r.Next(100000, 1000000);
            await _codeCache.SetRecordAsync($"code_{userId}", code.ToString());
        }

        public async Task<string> GetConfirmationCode(Guid userId) {
            return await _codeCache.GetRecordAsync<string>($"code_{userId}");
        }
    }
}