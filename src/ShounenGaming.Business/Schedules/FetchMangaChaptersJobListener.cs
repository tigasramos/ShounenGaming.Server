using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Interfaces.Mangas;

namespace ShounenGaming.Business.Schedules
{
    
    public class FetchMangaChaptersJobListener : IInvocable
    {
        private readonly IServiceProvider services;

        public FetchMangaChaptersJobListener(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task Invoke()
        {
            try
            {
                using var scope = services.CreateScope();

                var queue = scope.ServiceProvider.GetRequiredService<IFetchMangasQueue>();
                var mangaId = queue.Dequeue();

                var mangaService = scope.ServiceProvider.GetRequiredService<IMangaService>();
                await mangaService.UpdateMangaChapters(mangaId);

            }
            catch (Exception ex)
            {
                Log.Error($"Error Fetching Manga Chapters: {ex.Message}\n{ex.StackTrace}");
            }

            await Task.Delay(2000);
            await Invoke();
        }
    }
}
