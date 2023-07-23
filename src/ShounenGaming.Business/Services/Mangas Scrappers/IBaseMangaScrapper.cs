using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    internal interface IBaseMangaScrapper
    {
        string GetBaseURLForManga();
        MangaTranslationEnumDTO GetLanguage();
        Task<List<MangaSourceDTO>> GetAllMangasByPage(int page);
        Task<List<MangaSourceDTO>> SearchManga(string name);
        Task<ScrappedManga> GetManga(string urlPart);
        Task<List<string>> GetChapterImages(string urlPart);
        virtual Dictionary<string, string> GetImageHeaders() { return null; }
        MangaSourceEnumDTO GetMangaSourceEnumDTO();
    }
}
