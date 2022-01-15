using Academatica.Api.Auth.AuthManagement;
using Academatica.Api.Auth.Configuration;
using Academatica.Api.Auth.Data;
using Academatica.Api.Auth.Services;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Academatica.Api.Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddTransient<DbInitializer>();
            services.AddTransient<Config>();
            services.AddTransient<IEmailSender, EmailService>();

            string connectionString = Configuration.GetConnectionString("AuthDbConnection");

            services.AddOptions();
            services.Configure<ConfigurationManager>(Configuration.GetSection("ConfigurationManager"));
            services.Configure<MailConfig>(Configuration.GetSection("MailConfig"));

            services.AddDbContext<AcadematicaDbContext>(options =>
            {
                options.UseNpgsql(connectionString, psql => psql.MigrationsAssembly(migrationsAssembly));
            });

            services.AddIdentity<User, AcadematicaRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password = new PasswordOptions
                {
                    RequireDigit = true,
                    RequiredLength = 6,
                    RequireLowercase = true,
                    RequireUppercase = true,
                    RequireNonAlphanumeric = false
                };
            }).AddEntityFrameworkStores<AcadematicaDbContext>().AddDefaultTokenProviders();

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
            }).AddAspNetIdentity<User>().AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(connectionString, psql => psql.MigrationsAssembly(migrationsAssembly));
            }).AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(connectionString, psql => psql.MigrationsAssembly(migrationsAssembly));

                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 15;
            }).AddDeveloperSigningCredential().AddResourceOwnerValidator<ResourceOwnerPasswordValidator<User>>(); ;

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academatica.Api.Auth", Version = "v0.2" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academatica.Api.Auth v0.2"));
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseIdentityServer();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
