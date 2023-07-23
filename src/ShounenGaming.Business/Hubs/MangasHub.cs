using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Hubs
{

    //TODO
    public interface IMangasHubClient
    {
        Task MangaAdded();
        Task ChapterAdded();
        Task SendMangasQueue(List<QueuedMangaDTO> queuedMangas);
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
