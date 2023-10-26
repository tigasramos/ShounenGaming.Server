using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;

namespace ShounenGaming.Business.Schedules
{
    public class DownloadChapterImagesJob : IInvocable
    {
        private readonly IServiceProvider services;

        public DownloadChapterImagesJob(IServiceProvider services)
        {
            this.services = services;
        }


        public async Task Invoke()
        {
            try
            {
                using var scope = services.CreateScope();
                var mangaService = scope.ServiceProvider.GetRequiredService<IMangaService>();
                await mangaService.DownloadImages();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Downloading Chapter Images");
            }
        }
    }
}
