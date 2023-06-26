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
        Task<Manga?> GetByMALId(long malId);
        Task<Manga?> GetByALId(long alId);
        Task<List<Manga>> GetPopularMangas();
        Task<List<Manga>> GetFeaturedMangas();
        Task<List<Manga>> SearchManga(int page, string? name, int? userId);
        Task<int> GetAllCount(string? name, int? userId);
        Task<List<Manga>> GetRecentlyAddedMangas();
        Task<List<MangaChapter>> GetRecentlyReleasedChapters();
        Task<Manga?> GetByChapter(int chapterId);
        Task<Manga?> ClearSources(int mangaId);
        Task<Manga?> ClearTranslations(int mangaId, IEnumerable<MangaTranslation> translations);
    }
}
