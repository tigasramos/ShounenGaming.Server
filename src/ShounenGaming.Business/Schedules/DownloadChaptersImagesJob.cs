using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;

namespace ShounenGaming.Business.Schedules
{
    public class DownloadChaptersImagesJob : IInvocable
    {
        private readonly IServiceProvider services;

        public DownloadChaptersImagesJob(IServiceProvider services)
        {
            this.services = services;
        }


        public async Task Invoke()
        {
            try
            {
                Log.Information($"Started Downloading Mangas Chapters Images");

                using var scope = services.CreateScope();
                var mangaService = scope.ServiceProvider.GetRequiredService<IMangaService>();
                await mangaService.DownloadImagesFromAllMangas();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Downloading Mangas Chapters Images");
            }
        }
    }
}
