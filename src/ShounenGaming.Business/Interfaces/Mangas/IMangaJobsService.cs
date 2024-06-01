using ShounenGaming.Business.Helpers;

namespace ShounenGaming.Business.Interfaces.Mangas
{
    public interface IMangaJobsService
    {
        Task AddOrUpdateAllMangasMetadata();

        Task AddNewSeasonMangas();

        Task<int> UpdateAllMangasMetadata();

        Task AddAllMangasToChaptersQueue(); 
        Task UpdateMangaChapters(QueuedManga queuedManga);
    }
}
