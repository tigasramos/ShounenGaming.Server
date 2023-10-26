using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class MangaDexENScrapper : MangaDexAbstractScrapper, IBaseMangaScrapper
    {
        public MangaDexENScrapper(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public override MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.EN;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            return await GetMangaByLanguage(urlPart);
        }

        public override MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.MANGAS_DEX_EN;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            return await SearchMangasUnified(GetSearchQuery() + $"&title={name}");
        }
    }
}
