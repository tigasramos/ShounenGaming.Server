using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.DataAccess.Persistence;

namespace ShounenGaming.Common
{
    public static class AutomatedMigration
    {
        public static WebApplication MigrateDatabase(this WebApplication webApp)
        {
            using (var scope = webApp.Services.CreateScope())
            {
                using var appContext = scope.ServiceProvider.GetRequiredService<ShounenGamingContext>();
                try
                {
                    if (appContext.Database.IsNpgsql())
                    {
                        Log.Information("Applying Migrations");
                        appContext.Database.Migrate();
                        Log.Information("Done with Migrations");
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex.Message);
                    throw;
                }
            }
            return webApp;
        }
    }
}
