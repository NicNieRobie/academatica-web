using Academatica.Api.Auth.AuthManagement;
using Academatica.Api.Auth.Data;
using Academatica.Api.Common.Configuration;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Common.Services;
using AspNetCore.Yandex.ObjectStorage.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Reflection;

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
            services.AddControllers();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddTransient<DbInitializer>();
            services.AddTransient<Config>();
            services.AddTransient<IEmailSender, EmailService>();
            services.AddCors();

            services.AddYandexObjectStorage(options =>
            {
                options.BucketName = Configuration["ObjStorageConfig:BucketName"];
                options.AccessKey = Configuration["ObjStorageConfig:AccessKey"];
                options.SecretKey = Configuration["ObjStorageConfig:SecretKey"];
            });

            string connectionString = Configuration.GetConnectionString("AuthDbConnection");

            services.AddOptions();
            services.Configure<Configuration.ConfigurationManager>(Configuration.GetSection("ConfigurationManager"));
            services.Configure<MailConfig>(Configuration.GetSection("MailConfig"));

            services.AddDbContext<AcadematicaDbContext>(options =>
            {
                options.UseNpgsql(connectionString, psql => {
                    psql.MigrationsAssembly(migrationsAssembly);
                });
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
                options.IssuerUri = Configuration["Url"];
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
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "Academatica.Api.Auth.xml");
                c.IncludeXmlComments(filePath);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academatica.Api.Auth", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academatica.Api.Auth v1"));
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
