using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal interface IBaseMangaScrapper
    {
        string GetBaseURLForManga();
        string GetLanguage();
        Task<List<ScrappedSimpleManga>> GetAllMangas();
        Task<List<ScrappedSimpleManga>> SearchManga(string name);
        Task<ScrappedManga> GetManga(string urlPart);
        Task<List<string>> GetChapterImages(string urlPart);

        MangaSourceEnumDTO GetMangaSourceEnumDTO();
    }
}
