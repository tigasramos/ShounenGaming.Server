using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    internal class MangaDexPTScrapper : MangaDexAbstractScrapper, IBaseMangaScrapper
    {
        public async Task<List<MangaSourceDTO>> GetAllMangasByPage(int page)
        {
            return await SearchMangasUnified(GetSearchQuery(limit: 50, offset: page * 50));
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
