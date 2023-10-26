using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Schedules
{
    public class FetchSeasonMangasJob : IInvocable
    {
        private readonly IServiceProvider services;

        public FetchSeasonMangasJob(IServiceProvider services)
        {
            this.services = services;
        }


        public async Task Invoke()
        {
            try
            {
                using var scope = services.CreateScope();
                var mangaService = scope.ServiceProvider.GetRequiredService<IMangaService>();
                await mangaService.FetchSeasonMangas();
                await Task.Delay(60000);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Fetching Season Mangas");
            }
        }
    }
}
