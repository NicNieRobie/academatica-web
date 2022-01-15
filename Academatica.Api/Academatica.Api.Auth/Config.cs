using Academatica.Api.Auth.Configuration;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Auth
{
    public class Config
    {
        private readonly ConfigurationManager _configurationManager;

        public Config(IOptions<ConfigurationManager> options)
        {
            _configurationManager = options.Value;
        }

        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(_configurationManager.ApiName, _configurationManager.ApiName)
                {
                    UserClaims =
                    {
                        JwtClaimTypes.Subject,
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Role
                    }
                }
            };
        }

        public IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope(_configurationManager.ApiName, _configurationManager.ApiName)
            };
        }

        public IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = _configurationManager.ClientId,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequireClientSecret = false,
                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AllowedScopes =
                    {
                        _configurationManager.ApiName,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                }
            };
        }
    }
}
