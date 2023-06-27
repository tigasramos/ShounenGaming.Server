using Coravel.Invocable;
using Coravel.Queuing.Interfaces;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Schedules
{
    public class AddOrUpdateMangasMetadataJob : IInvocable
    {
        private readonly IMangaService _mangaService;

        public AddOrUpdateMangasMetadataJob(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }


        public async Task Invoke()
        {
            try
            {  
                await _mangaService.AddOrUpdateAllMangasMetadata();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Updating Mangas Metadata: {ex.Message}");
            }
        }
    }
}
