using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Mappers;
using ShounenGaming.Business.Services.Base;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Persistence;
using ShounenGaming.DataAccess.Repositories.Base;

namespace ShounenGaming.Common
{
    public static class DIConfigurations
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment, string assemblyName)
        {
            services.AddControllers();

            services.AddSwagger(assemblyName);
            services.AddAutoMapper(typeof(UserMapper).Assembly);
            services.AddSQLDatabase(configuration, environment);
            services.AddRepositories();
            services.AddServices(environment, configuration);

            services.AddEndpointsApiExplorer();

        }

        public static void ConfigureApp(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            app.UseCors(corsPolicyBuilder =>
                corsPolicyBuilder.AllowAnyOrigin()
                                 .AllowAnyMethod()
                                 .AllowAnyHeader());

            app.UseSwagger();
            app.UseSwaggerUI();

            //TODO: Remove when deploying
            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }


        #region Private
        private static void AddSwagger(this IServiceCollection services, string assemblyName)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShounenGaming.Server", Version = "v1" });


                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });


                var xmlFile = $"{assemblyName}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }
        private static void AddServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
        {
            services.AddTransient<IUserService, UserService>();
        }
        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
        }
        private static void AddSQLDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddDbContext<DbContext, ShounenGamingContext>(opt =>
            {
                opt.UseInMemoryDatabase("ShounenGamingDB");
                opt.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
        }
        #endregion
    }
}
