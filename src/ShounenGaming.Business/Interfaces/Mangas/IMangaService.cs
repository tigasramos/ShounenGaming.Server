using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
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


        /// <summary>
        /// Gets the most Popular Mangas
        /// </summary>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetPopularMangas();

        /// <summary>
        /// Gets the new added Mangas
        /// </summary>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetRecentlyAddedMangas();

        /// <summary>
        /// Gets the new added Chapters
        /// </summary>
        /// <returns></returns>
        Task<List<ChapterReleaseDTO>> GetRecentlyReleasedChapters();

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

        /// <summary>
        /// Gets all Manga Tags
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetMangaTags();

        /// <summary>
        /// Gets a Manga Translation by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MangaTranslationDTO> GetMangaTranslation(int mangaId, int chapterId, MangaTranslationEnumDTO translation);

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
        /// Gets all Mangas from a Source by Page
        /// </summary>
        /// <param name="source"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<List<MangaSourceDTO>> GetAllMangasFromSourceByPage(MangaSourceEnumDTO source, int page);

        /// <summary>
        /// Adds Sources to an already existing Manga with that Id
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="mangas"></param>
        /// <returns></returns>
        Task<List<MangaSourceDTO>> LinkSourcesToManga(int mangaId, List<MangaSourceDTO> mangas);

       

        /// <summary>
        /// Adds a Task to the queue to get all Chapters from a Manga by its Sources
        /// </summary>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        Task StartMangaChaptersUpdate(int mangaId);

        /// <summary>
        /// Delete everything related to that Source (when source is not working anymore)
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="oldSource"></param>
        /// <returns></returns>
        Task ChangeSourceFromManga(int mangaId, MangaSourceEnumDTO oldSource);


        #region Jobs
        /// <summary>
        /// Updates all Mangas Metadata
        /// </summary>
        /// <returns></returns>
        internal Task<int> UpdateMangasMetadata();

        /// <summary>
        /// Adds all tracked Mangas to the Queue (fetches new chapters or translations)
        /// </summary>
        /// <returns></returns>
        internal Task UpdateMangasChapters();
        internal Task LoadInitialMangas();
        internal Task UpdateMangaChapters(int mangaId);
        #endregion
    }
}
