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
    public class UpdateMangasInvocable : IInvocable
    {
        private readonly IMangaService _mangaService;

        public UpdateMangasInvocable(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }


        public async Task Invoke()
        {
            try
            {
                await _mangaService.UpdateMangasChapters();
                Log.Information($"UpdateMangasInvocable ran successfully!");
            }
            catch(Exception ex)
            {
                Log.Error($"Running UpdateMangasInvocable: {ex.Message}");
            }
        }
    }
}
