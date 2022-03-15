using Academatica.Api.Common.Configuration;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Users.Services;
using Academatica.Api.Users.Services.Grpc;
using Academatica.Api.Users.Services.RabbitMQ;
using AspNetCore.Yandex.ObjectStorage.Extensions;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using System.IO;

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
            IdentityModelEventSource.ShowPII = true;
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

            services.AddIdentity<User, AcadematicaRole>().AddEntityFrameworkStores<AcadematicaDbContext>().AddDefaultTokenProviders();

            string redisConnectionString = Configuration.GetConnectionString("Redis");
            System.Console.WriteLine("----------------> REDIS: " + redisConnectionString);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "UsersSvc_";
            });

            services.AddHangfire(x =>
            {
                x.UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions()
                {
                    SchemaName = "UserSvcHangfire"
                });
            });
            services.AddHangfireServer(x => x.WorkerCount = 2);

            services.AddGrpc();
            services.AddTransient<IAchievementsManager, AchievementsManager>();
            services.AddTransient<IConfirmationCodeManager, ConfirmationCodeManager>();
            services.AddTransient<IUserEmailService, UserEmailService>();
            services.AddTransient<IUserStatsManager, UserStatsManager>();
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
            services.AddSwaggerGen(c =>
            {
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "Academatica.Api.Users.xml");
                c.IncludeXmlComments(filePath);
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
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GrpcAchievementsService>();
            });

            RecurringJob.AddOrUpdate<IUserStatsManager>("buoysupdatejob", x => x.UpdateUsersBuoys(), @"0 * * * *");
            RecurringJob.AddOrUpdate<IUserStatsManager>("streakupdatejob", x => x.UpdateUsersDayStreaks(), @"0 0 * * *");
            RecurringJob.AddOrUpdate<IUserStatsManager>("weekexpupdatejob", x => x.UpdateUsersExpThisWeek(), @"0 0 * * 0");
        }
    }
}
