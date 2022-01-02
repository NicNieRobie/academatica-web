using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartMath.Api.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Auth.Data
{
    public class AuthDbContext : IdentityDbContext<User, AuthRole, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
