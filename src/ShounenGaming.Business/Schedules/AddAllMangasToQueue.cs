using Coravel.Invocable;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Schedules
{
    public class AddAllMangasToQueue : IInvocable
    {
        private readonly IMangaService _mangaService;

        public AddAllMangasToQueue(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }


        public async Task Invoke()
        {
            try
            {
                Log.Information($"Started Fetching Mangas Chapters");
                await _mangaService.UpdateMangasChapters();
            }
            catch(Exception ex)
            {
                Log.Error($"Error Fetching Mangas Chapters: {ex.Message}");
            }
        }
    }
}
