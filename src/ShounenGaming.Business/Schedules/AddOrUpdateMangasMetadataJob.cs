using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;

namespace ShounenGaming.Business.Schedules
{
    public class AddOrUpdateMangasMetadataJob : IInvocable
    {
        private readonly IServiceProvider services;

        public AddOrUpdateMangasMetadataJob(IServiceProvider services)
        {
            this.services = services;
        }


        public async Task Invoke()
        {
            try
            {
                using var scope = services.CreateScope();
                var mangaService = scope.ServiceProvider.GetRequiredService<IMangaJobsService>();
                await mangaService.AddOrUpdateAllMangasMetadata();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Updating Mangas Metadata");
            }
        }
    }
}
