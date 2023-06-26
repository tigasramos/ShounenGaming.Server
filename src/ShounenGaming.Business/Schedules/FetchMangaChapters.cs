using Coravel.Invocable;
using Serilog;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Interfaces.Mangas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Schedules
{
    
    public class FetchMangaChapters : IInvocable
    {
        private readonly IFetchMangasQueue _queue;
        private readonly IMangaService _mangaService;

        public FetchMangaChapters(IMangaService mangaService, IFetchMangasQueue queue)
        {
            _mangaService = mangaService;
            _queue = queue;
        }


        public async Task Invoke()
        {
            try
            {
                var mangaId = _queue.Dequeue();
                await _mangaService.UpdateMangaChapters(mangaId);
                await Task.Delay(1000);
                await Invoke();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Fetching Manga Chapters: {ex.Message}");
            }
        }
    }
}
