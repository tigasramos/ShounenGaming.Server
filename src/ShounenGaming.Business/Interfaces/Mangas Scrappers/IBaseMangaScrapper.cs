using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal interface IBaseMangaScrapper
    {
        Task<List<ScrappedSimpleManga>> GetAllMangas();
        Task<ScrappedManga> GetManga(string urlPart);
        Task<List<string>> GetChapterImages(string urlPart);
    }
}
