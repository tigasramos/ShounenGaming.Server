using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class MangaDexPTScrapper : MangaDexAbstractScrapper, IBaseMangaScrapper
    {
        public MangaDexPTScrapper(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public override MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            return await GetMangaByLanguage(urlPart);
        }

        public override MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.MANGAS_DEX_PT;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            return await SearchMangasUnified(GetSearchQuery() + $"&title={name}");
        }

    }
}
