using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;

namespace ShounenGaming.Business.Schedules
{
    public class FetchAllMangasChaptersJob : IInvocable
    {
        private readonly IServiceProvider services;

        public FetchAllMangasChaptersJob(IServiceProvider services)
        {
            this.services = services;
        }


        public async Task Invoke()
        {
            try
            {
                Log.Information($"Started Adding Mangas Chapters to Queue");

                using var scope = services.CreateScope();
                var mangaService = scope.ServiceProvider.GetRequiredService<IMangaService>();
                await mangaService.UpdateMangasChapters();
            }
            catch(Exception ex)
            {
                Log.Error($"Error Adding Mangas Chapters to Queue: {ex.Message}");
            }
        }
    }
}
