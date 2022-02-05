using Academatica.Api.Common.Configuration;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Common.Services;
using Academatica.Api.Users.Configuration;
using Academatica.Api.Users.Services;
using Academatica.Api.Users.Services.SyncDataServices.Http;
using AspNetCore.Yandex.ObjectStorage.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users
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
            services.AddAuthorization(options => options.AddPolicy("User", policy => policy.RequireClaim("role", "User")))
                    .AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireClaim("role", "Admin")));
            services.AddCors();
            services.AddControllers();

            services.AddYandexObjectStorage(options =>
            {
                options.BucketName = Configuration["ObjStorageConfig:BucketName"];
                options.AccessKey = Configuration["ObjStorageConfig:AccessKey"];
                options.SecretKey = Configuration["ObjStorageConfig:SecretKey"];
            });

            string connectionString = Configuration.GetConnectionString("UsersDbConnection");
            services.AddDbContext<AcadematicaDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddOptions();
            var section = Configuration.GetSection("AuthConfiguration");
            services.Configure<AuthConfiguration>(section);
            services.Configure<MailConfig>(Configuration.GetSection("MailConfig"));

            AuthConfiguration settings = section.Get<AuthConfiguration>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = settings.Authority;
                    options.RequireHttpsMetadata = false;
                    options.ApiName = settings.ApiResourceName;
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

            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "UsersSvc_";
                options.Configuration = Configuration.GetConnectionString("Redis");
            });

            services.AddTransient<IConfirmationCodeManager, ConfirmationCodeManager>();
            services.AddTransient<IEmailSender, EmailService>();
            services.AddHttpClient<IAuthDataClient, HttpAuthDataClient>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academatica.Api.Users", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academatica.Api.Users v1"));
                IdentityModelEventSource.ShowPII = true;
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
