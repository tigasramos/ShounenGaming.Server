using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.DataAccess.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        appContext.Database.Migrate();
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
