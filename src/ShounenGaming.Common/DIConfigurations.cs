using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ShounenGaming.Business.Hubs;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Interfaces.Tierlists;
using ShounenGaming.Business.Mappers;
using ShounenGaming.Business.Services.Base;
using ShounenGaming.Business.Services.Tierlists;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Tierlists;
using ShounenGaming.DataAccess.Persistence;
using ShounenGaming.DataAccess.Repositories.Base;
using ShounenGaming.DataAccess.Repositories.Tierlists;
using System.Text;

namespace ShounenGaming.Common
{
    public static class DIConfigurations
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment, string assemblyName)
        {
            services.AddControllers();
            services.AddSignalR();

            services.AddSwagger(assemblyName);
            services.AddAutoMapper(typeof(UserMapper).Assembly);
            services.AddSQLDatabase(configuration, environment);
            services.AddRepositories();
            services.AddServices(environment, configuration);
            services.AddJwt(configuration);

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

            app.UseMiddleware<ExceptionMiddleware>();

            //TODO: Remove when deploying
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapSignalRHubs();

            app.MapControllers();

            app.Run();
        }


        #region Private
        private static void MapSignalRHubs(this WebApplication app)
        {
            app.MapHub<AuthHub>("/authHub");
        }
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
        private static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("Admin", policy => policy.RequireClaim("Role", new string[] { Core.Entities.Base.Enums.RolesEnum.ADMIN.ToString() }));
                opt.AddPolicy("Mod", policy => policy.RequireClaim("Role", new string[] { Core.Entities.Base.Enums.RolesEnum.ADMIN.ToString(), Core.Entities.Base.Enums.RolesEnum.MOD.ToString() }));
                opt.AddPolicy("Bot", policy => policy.RequireClaim("Role", new string[] { "Bot" }));
            });

            var secretKey = "TODO_SUPER_HUGE_SECRET_KEY"; //TODO: Change to Settings
            var key = Encoding.ASCII.GetBytes(secretKey);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

        }

        private static void AddServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
        {
            //Others
            services.AddMemoryCache();

            //Hubs
            services.AddTransient<AuthHub>();

            //Services
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<ITierlistService, TierlistService>();
        }
        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IBotRepository, BotRepository>();
            services.AddTransient<IFileDataRepository, FileDataRepository>();

            services.AddTransient<ITierChoiceRepository, TierChoiceRepository>();
            services.AddTransient<ITierlistCategoryRepository, TierlistCategoryRepository>();
            services.AddTransient<ITierlistItemRepository, TierlistItemRepository>();
            services.AddTransient<ITierlistRepository, TierlistRepository>();
            services.AddTransient<ITierRepository, TierRepository>();
            services.AddTransient<IUserTierlistRepository, UserTierlistRepository>();
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
