using Academatica.Api.Auth.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using Academatica.Api.Auth.AuthManagement;

namespace Academatica.Api.Auth.Data
{
    public class DbInitializer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<AcadematicaRole> _roleManager;

        private readonly ConfigurationManager _configurationManager;

        private readonly AcadematicaDbContext _dbContext;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly PersistedGrantDbContext _grantDbContext;

        private readonly Config _config;

        public DbInitializer(IOptions<ConfigurationManager> options,
            UserManager<User> userManager,
            RoleManager<AcadematicaRole> roleManager,
            AcadematicaDbContext dbContext,
            ConfigurationDbContext configurationDbContext,
            PersistedGrantDbContext grantDbContext,
            Config config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configurationManager = options.Value;
            _dbContext = dbContext;
            _configurationDbContext = configurationDbContext;
            _grantDbContext = grantDbContext;
            _config = config;
        }

        public void InitializeDb()
        {
            _dbContext.Database.Migrate();
            _configurationDbContext.Database.Migrate();
            _grantDbContext.Database.Migrate();

            SeedUsers();

            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var client in _config.GetClients().ToList())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var identityResource in _config.GetIdentityResources().ToList())
                {
                    _configurationDbContext.IdentityResources.Add(identityResource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var apiResource in _config.GetApiResources().ToList())
                {
                    _configurationDbContext.ApiResources.Add(apiResource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiScopes.Any())
            {
                foreach (var apiScope in _config.GetApiScopes().ToList())
                {
                    _configurationDbContext.ApiScopes.Add(apiScope.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }
        }

        private void SeedUsers()
        {
            if (!_dbContext.Users.Any())
            {
                User user = new User
                {
                    UserName = "admin",
                    FirstName = "",
                    LastName = "",
                    Email = "vmpendischuk@edu.hse.ru",
                    RegisteredAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    ProfilePicUrl = null
                };

                var result = _userManager.CreateAsync(user, _configurationManager.AdminPass).GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    _roleManager.CreateAsync(new AcadematicaRole { Name = ServerRoles.Admin }).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new AcadematicaRole { Name = ServerRoles.User }).GetAwaiter().GetResult();

                    _userManager.AddToRoleAsync(user, ServerRoles.Admin).GetAwaiter().GetResult();
                } else
                {
                    throw new Exception(string.Join("; ", result.Errors.Select(x => x.Description)));
                }

                result = _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, ""),
                    new Claim(JwtClaimTypes.GivenName, ""),
                    new Claim(JwtClaimTypes.FamilyName, ""),
                    new Claim(JwtClaimTypes.Email, "vmpendischuk@edu.hse.ru"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean)
                }).GetAwaiter().GetResult();

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ", result.Errors.Select(x => x.Description)));
                }
            }
        }
    }
}
