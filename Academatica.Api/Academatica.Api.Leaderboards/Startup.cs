using Academatica.Api.Common.Configuration;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Leaderboards.EventProcessing;
using Academatica.Api.Leaderboards.Services;
using Academatica.Api.Leaderboards.Services.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.IO;

namespace Academatica.Api.Leaderboards
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

            services.AddOptions();
            var section = Configuration.GetSection("AuthConfiguration");
            services.Configure<AuthConfiguration>(section);

            AuthConfiguration settings = section.Get<AuthConfiguration>();

            string connectionString = Configuration.GetConnectionString("DbConnection");
            services.AddDbContext<AcadematicaDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

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

            var redis = ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis"));

            services.AddSingleton<IConnectionMultiplexer>(redis);
            services.AddTransient<ILeaderboardService, LeaderboardService>();
            services.AddTransient<ILeaderboardManager, LeaderboardManager>();
            services.AddTransient<IEventProcessor, EventProcessor>();
            services.AddHostedService<MessageBusSubscriber>();

            services.AddSwaggerGen(c =>
            {
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "Academatica.Api.Leaderboards.xml");
                c.IncludeXmlComments(filePath);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academatica.Api.Leaderboards", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academatica.Api.Leaderboards v1"));
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