using Coravel;
using JikanDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using ShounenGaming.Business;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Hubs;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Tierlists;
using ShounenGaming.Business.Mappers;
using ShounenGaming.Business.Schedules;
using ShounenGaming.Business.Services.Base;
using ShounenGaming.Business.Services.Mangas;
using ShounenGaming.Business.Services.Tierlists;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DataAccess.Interfaces.Tierlists;
using ShounenGaming.DataAccess.Persistence;
using ShounenGaming.DataAccess.Repositories.Base;
using ShounenGaming.DataAccess.Repositories.Mangas;
using ShounenGaming.DataAccess.Repositories.Tierlists;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ZiggyCreatures.Caching.Fusion;

namespace ShounenGaming.Common
{
    public static class DIConfigurations
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment, string assemblyName)
        {
            services.AddJwt(configuration);
            services.AddSwagger(assemblyName);
            services.AddAutoMapper(typeof(UserMapper).Assembly);
            services.AddSQLDatabase(configuration, environment);
            services.AddRepositories();
            services.AddServices(environment, configuration);

            services.AddHealthChecks();

            services.AddRateLimiter(options => {
                options.RejectionStatusCode = 429;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(), factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));

                options.AddFixedWindowLimiter("Auth", options => {
                    options.AutoReplenishment = true;
                    options.PermitLimit = 10;
                    options.Window = TimeSpan.FromMinutes(1);
                });
            });

            services.AddFusionCache()
                .WithDefaultEntryOptions(new FusionCacheEntryOptions
                {
                    Duration = TimeSpan.FromMinutes(30),
                });

            services.AddScheduler();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            services.AddSignalR();

            services.AddEndpointsApiExplorer();

        }

        public static void ConfigureApp(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            Log.Information($"Environment: {app.Environment.EnvironmentName}");

            //TODO: Check Coravel Cache Service
            app.Services.UseScheduler(scheduler =>
            {
                if (app.Environment.IsProduction())
                {
                    // Background Job that will listen the queue to Fetch New Chapters
                    scheduler.OnWorker("MangasChapters_Listener");
                    scheduler.Schedule<FetchMangaChaptersJobListener>().Monthly().RunOnceAtStart().PreventOverlapping("MangasChapters_Listener");

                    // Fetch Manga Metadata every 3h
                    scheduler.OnWorker("Mangas");
                    scheduler.Schedule<AddOrUpdateMangasMetadataJob>().Cron("20 */3 * * *").RunOnceAtStart().PreventOverlapping("MangasMetadata");
                    scheduler.Schedule<FetchSeasonMangasJob>().DailyAt(3, 30).RunOnceAtStart();
                    scheduler.Schedule<FetchAllMangasChaptersJob>().Cron("0 */2 * * *").RunOnceAtStart();

                } 
                else
                {
                    // Background Job that will listen the queue to Fetch New Chapters
                    scheduler.OnWorker("MangasChapters_Listener");
                    scheduler.Schedule<FetchMangaChaptersJobListener>().Monthly().RunOnceAtStart().PreventOverlapping("MangasChapters_Listener");

                    // Fetch Manga Metadata
                    scheduler.OnWorker("Mangas");
                    scheduler.Schedule<AddOrUpdateMangasMetadataJob>().DailyAt(1, 30).PreventOverlapping("MangasMetadata");
                    scheduler.Schedule<FetchAllMangasChaptersJob>().Monthly();
                    scheduler.Schedule<FetchSeasonMangasJob>().DailyAt(3, 0).RunOnceAtStart();

                }
                


            }).OnError((ex) => Log.Error($"Running some schedule: {ex.Message}")); ;

            app.UseCors(corsPolicyBuilder =>
                corsPolicyBuilder.AllowAnyOrigin()
                                 .AllowAnyMethod()
                                 .AllowAnyHeader());

            app.MapHealthChecks("/healthz");
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapSignalRHubs();

            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseRateLimiter();

            app.MapControllers();

            app.MigrateDatabase();

            app.Run();
        }


        #region Private
        private static void MapSignalRHubs(this WebApplication app)
        {
            app.MapHub<DiscordEventsHub>("/discordEventsHub");
            app.MapHub<MangasHub>("/mangasHub");
            app.MapHub<LobbiesHub>("/lobbiesHub");
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

            var secretKey = configuration.GetRequiredSection("settings").Get<AppSettings>()!.JwtSecret;
            var key = Encoding.ASCII.GetBytes(secretKey);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
                    ClockSkew = TimeSpan.Zero
                };

                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = string.Empty;

                        // Get Token from Headers
                        var authHeader = context.Request.Headers["Authorization"];
                        if (authHeader.Count > 0)
                        {
                            accessToken = authHeader[0]!.Split(" ")[1];
                        }

                        // Get Token from Query (SignalR)
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            accessToken = context.Request.Query["access_token"].ToString();
                        }

                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });

        }

        private static void AddServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
        {
            var settings = configuration.GetRequiredSection("Settings").Get<AppSettings>()!;
            //Others
            services.AddMemoryCache();
            services.AddSingleton(settings);
            services.AddSingleton<IFetchMangasQueue, FetchMangasQueue>();

            //Hubs
            services.AddTransient<DiscordEventsHub>();
            services.AddTransient<LobbiesHub>();

            //Schedules
            services.AddTransient<FetchAllMangasChaptersJob>();
            services.AddTransient<FetchMangaChaptersJobListener>();
            services.AddTransient<AddOrUpdateMangasMetadataJob>();
            services.AddTransient<FetchSeasonMangasJob>();

            //Services
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITierlistService, TierlistService>();
            services.AddTransient<IMangaService, MangaService>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IMangaUserDataService, MangaUserDataService>();
            services.AddTransient<IJikan, Jikan>();

            services.AddHttpContextAccessor();
        }
        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IServerMemberRepository, ServerMemberRepository>();

            services.AddTransient<ITierlistCategoryRepository, TierlistCategoryRepository>();
            services.AddTransient<ITierlistRepository, TierlistRepository>();
            services.AddTransient<IUserTierlistRepository, UserTierlistRepository>();

            services.AddTransient<IMangaRepository, MangaRepository>();
            services.AddTransient<IMangaUserDataRepository, MangaUserDataRepository>();
            services.AddTransient<IMangaWriterRepository, MangaWriterRepository>();
            services.AddTransient<IMangaTagRepository, MangaTagRepository>();
            services.AddTransient<IAddedMangaActionRepository, AddedMangaActionRepository>();
            services.AddTransient<IChangedChapterStateActionRepository, ChangedChapterStateActionRepository>();
            services.AddTransient<IChangedMangaStatusActionRepository, ChangedMangaStatusActionRepository>();
        }
        private static void AddSQLDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            var connectionString = configuration["ConnectionStrings:ShounenGamingDB"];
            
            services.AddDbContext<DbContext, ShounenGamingContext>(opt =>
            {
                opt.UseLazyLoadingProxies();
                opt.UseNpgsql(connectionString, x => x.MigrationsAssembly(Assembly.GetAssembly(typeof(ShounenGamingContext)).ToString()));
                opt.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
        }
        #endregion
    }
}
