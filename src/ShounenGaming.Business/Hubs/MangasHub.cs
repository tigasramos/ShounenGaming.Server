using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Services.Base;
using ShounenGaming.Core.Entities.Base.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Hubs
{

    //TODO
    public interface IMangasHubClient
    {
        Task MangaAdded();
        Task ChapterAdded();
    }

    [Authorize]
    public class MangasHub : Hub<IMangasHubClient>
    {
        private readonly IMangaService _mangaService;

        public MangasHub(IMangaService mangaService)
        {
            _mangaService = mangaService;
        }

        public async Task AddManga(MangaMetadataSourceEnumDTO source, long mangaId, string discordId)
        {
            await _mangaService.AddManga(source, mangaId, discordId);
        }
    }

}
