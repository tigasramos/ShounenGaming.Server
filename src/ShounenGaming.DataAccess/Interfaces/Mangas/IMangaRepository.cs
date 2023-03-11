using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Mangas
{
    public interface IMangaRepository : IBaseRepository<Manga>
    {
        Task<List<Manga>> GetPopularMangas(int count);
        Task<List<Manga>> SearchMangaByName(string name);
        Task<List<Manga>> GetRecentlyAddedMangas();
        Task<List<MangaChapter>> GetRecentlyReleasedChapters();
    }
}
