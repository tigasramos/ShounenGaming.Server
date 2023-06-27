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
    public class FetchAllMangasChaptersJob : IInvocable
    {
        private readonly IMangaService _mangaService;

        public FetchAllMangasChaptersJob(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }


        public async Task Invoke()
        {
            try
            {
                Log.Information($"Started Adding Mangas Chapters to Queue");
                await _mangaService.UpdateMangasChapters();
            }
            catch(Exception ex)
            {
                Log.Error($"Error Adding Mangas Chapters to Queue: {ex.Message}");
            }
        }
    }
}
