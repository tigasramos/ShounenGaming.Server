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
        Task<bool> MangaExistsByMAL(long malId);
        Task<bool> MangaExistsByAL(long alId);
        Task<Manga?> GetByMALId(long malId);
        Task<Manga?> GetByALId(long alId);
        Task<List<Manga>> GetWaitingMangas();
        Task<List<Manga>> GetPopularMangas(bool includeNSFW = true);
        Task<List<Manga>> GetFeaturedMangas(bool includeNSFW = true);
        Task<List<Manga>> SearchManga(int page, bool includeNSFW, string? name, int? userId);
        Task<int> GetAllCount(bool includeNSFW, string? name, int? userId);
        Task<List<Manga>> GetRecentlyAddedMangas();
        Task<List<Manga>> GetRecentlyReleasedChapters(bool includeNSFW = true);
        Task<Manga?> GetByChapter(int chapterId);
        Task<Manga?> GetByChapters(List<int> chaptersIds);
        Task<Manga?> ClearSources(int mangaId);
        Task<Manga?> ClearTranslations(int mangaId, IEnumerable<MangaTranslation> translations);
    }
}
