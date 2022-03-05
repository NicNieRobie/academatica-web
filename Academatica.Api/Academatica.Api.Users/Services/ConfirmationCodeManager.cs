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

        public async Task<string> GetEmailConfirmationCode(Guid userId) {
            return await _codeCache.GetRecordAsync<string>($"code_{userId}_email");
        }

        public async Task<string> CreateEmailConfirmationCode(Guid userId)
        {
            await _codeCache.RemoveAsync($"code_{userId}_email");

            Random r = new Random();
            var code = r.Next(100000, 1000000);
            await _codeCache.SetRecordAsync($"code_{userId}_email", code.ToString());
            return code.ToString();
        }

        public async Task RemoveEmailConfirmationCode(Guid userId)
        {
            await _codeCache.RemoveAsync($"code_{userId}_email");
        }

        public async Task<string> CreatePasswordConfirmationCode(Guid userId)
        {
            await _codeCache.RemoveAsync($"code_{userId}_pass");

            Random r = new Random();
            var code = r.Next(100000, 1000000);
            await _codeCache.SetRecordAsync($"code_{userId}_pass", code.ToString());
            return code.ToString();
        }

        public async Task<string> GetPasswordConfirmationCode(Guid userId)
        {
            return await _codeCache.GetRecordAsync<string>($"code_{userId}_pass");
        }

        public async Task RemovePasswordConfirmationCode(Guid userId)
        {
            await _codeCache.RemoveAsync($"code_{userId}_pass");
        }
    }
}