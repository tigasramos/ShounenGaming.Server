using Microsoft.Extensions.DependencyInjection;
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
        public static async Task MigrateAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ShounenGamingContext>();

            //TODO: Depends on DB
            //if (context.Database.IsNpgsql())
            //{
            //    await context.Database.MigrateAsync();
            //}

        }
    }
}
