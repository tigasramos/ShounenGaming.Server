using ShounenGaming.Business.Helpers;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.Business.Interfaces.Mangas
{
    public interface IMangaService
    {
        /// <summary>
        /// Gets a Manga by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MangaDTO> GetMangaById(int id);

        /// <summary>
        /// Gets the Manga Sources by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<MangaSourceDTO>> GetMangaSourcesById(int id);

       
        /// <summary>
        /// Searches Mangas
        /// </summary>
        /// <returns></returns>
        Task<PaginatedResponse<MangaInfoDTO>> SearchMangas(SearchMangaQueryDTO query, int page, int? userId = null);
        
        #region Lists of Mangas
        /// <summary>
        /// Gets the Mangas from the Anime Season
        /// </summary>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetSeasonMangas();

        /// <summary>
        /// Gets the Mangas on Planned with no Sources
        /// </summary>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetWaitingMangas();

        /// <summary>
        /// Gets the most Popular Mangas
        /// </summary>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetPopularMangas(int? userId = null);

        /// <summary>
        /// Gets the new added Mangas
        /// </summary>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetRecentlyAddedMangas();

        /// <summary>
        /// Gets the new added Chapters
        /// </summary>
        /// <returns></returns>
        Task<List<LatestReleaseMangaDTO>> GetRecentlyReleasedChapters(int? userId = null);
        #endregion

        #region Writers
        /// <summary>
        /// Get Manga Writer by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MangaWriterDTO> GetMangaWriterById(int id);

        /// <summary>
        /// Gets all Manga Writers
        /// </summary>
        /// <returns></returns>
        Task<List<MangaWriterDTO>> GetMangaWriters();
        #endregion

        #region Tags
        /// <summary>
        /// Gets Mangas from Tags
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetMangasFromTag(string tag, int? userId = null);

        /// <summary>
        /// Gets all Manga Tags
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetMangaTags();
        #endregion

        /// <summary>
        /// Gets a Manga Translation by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MangaTranslationDTO?> GetMangaTranslation(int userId, int mangaId, int chapterId, MangaTranslationEnumDTO translation);

        /// <summary>
        /// Adds a Manga from a Metadata Source with that Id
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, int userId);

        /// <summary>
        /// Adds a Manga from a Metadata Source with that DiscordId
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, string discordId);


        /// <summary>
        /// Searches a Manga from MyAnimeList by its Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<MangaMetadataDTO>> SearchMangaMetadata(MangaMetadataSourceEnumDTO source, string name);

        /// <summary>
        /// Searches several Mangas in all Sources available by its Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<MangaSourceDTO>> SearchMangaSource(string name);

        /// <summary>
        /// Adds Sources to an already existing Manga with that Id
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="mangas"></param>
        /// <returns></returns>
        Task<List<MangaSourceDTO>> LinkSourcesToManga(int mangaId, List<MangaSourceDTO> mangas);


        /// <summary>
        /// Gets Recommendations for Specific Users
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetMangaRecommendations(int userId);


        /// <summary>
        /// Gets Recommendations for Specific Users not yet added in DB (MAL and AL)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<MangaMetadataDTO>> SearchMangaRecommendations(int userId);



        /// <summary>
        /// Adds a Task to the queue to get all Chapters from a Manga by its Sources
        /// </summary>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        Task StartMangaChaptersUpdate(int mangaId, int userId);

        /// <summary>
        /// Gets the Status of the Queue that's fetching chapters
        /// </summary>
        /// <returns></returns>
        Task<List<QueuedMangaDTO>> GetQueueStatus();

        Task FixDuplicatedChapters();
    }
}
