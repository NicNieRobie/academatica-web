using Academatica.Api.Common.Configuration;
using Academatica.Api.Common.Data;
using AspNetCore.Yandex.ObjectStorage.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
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
using System.Threading.Tasks;

namespace Academatica.Api.Course
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
            services.AddAuthorization(options => options.AddPolicy("User", policy => policy.RequireClaim("role", "User")))
                    .AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireClaim("role", "Admin")));
            services.AddCors();

            services.AddYandexObjectStorage(options =>
            {
                options.BucketName = Configuration["ObjStorageConfig:BucketName"];
                options.AccessKey = Configuration["ObjStorageConfig:AccessKey"];
                options.SecretKey = Configuration["ObjStorageConfig:SecretKey"];
            });

            string connectionString = Configuration.GetConnectionString("CourseDbConnection");
            services.AddDbContext<AcadematicaDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddOptions();
            var section = Configuration.GetSection("AuthConfiguration");
            services.Configure<AuthConfiguration>(section);
            AuthConfiguration settings = section.Get<AuthConfiguration>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = settings.Authority;
                    options.RequireHttpsMetadata = false;
                    options.ApiName = settings.ApiResourceName;
                });

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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academatica.Api.Course v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
