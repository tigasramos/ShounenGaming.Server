using Coravel.Invocable;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Schedules
{
    public class UpdateMangasMetadata : IInvocable
    {
        private readonly IMangaService _mangaService;

        public UpdateMangasMetadata(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }


        public async Task Invoke()
        {
            try
            {
                Log.Information($"Started Updating Mangas Metadata");
                var updatedMangas = await _mangaService.UpdateMangasMetadata();
                Log.Information($"Finished Updating Mangas Metadata: {updatedMangas} Mangas Updated");
            }
            catch (Exception ex)
            {
                Log.Error($"Error Updating Mangas Metadata: {ex.Message}");
            }
        }
    }
}
