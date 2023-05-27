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
        /// Searches Mangas
        /// </summary>
        /// <returns></returns>
        Task<PaginatedResponse<MangaDTO>> SearchMangas();

        /// <summary>
        /// Searches a Manga by its Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<PaginatedResponse<MangaDTO>> SearchMangasByName(string name);

        /// <summary>
        /// Searches a Manga by a Tag
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        Task<PaginatedResponse<MangaDTO>> SearchMangasByTags(List<string> tags);

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
        /// Searches a Manga from MyAnimeList by its Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<JikanDotNet.Manga>> SearchMangaMetaData(string name);

        /// <summary>
        /// Searches several Mangas in all Sources available by its Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<ScrappedSimpleManga>> SearchMangaSource(string name);

        /// <summary>
        /// Gets all Mangas from a Source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<List<ScrappedSimpleManga>> GetAllMangasFromSource(MangaSourceEnumDTO source);

        /// <summary>
        /// Adds a new Manga to the DB (if not exists) and links it to the Sources selected
        /// </summary>
        /// <param name="myAnimeListMangaId"></param>
        /// <param name="mangas"></param>
        /// <returns></returns>
        Task<MangaDTO> LinkSourcesToManga(int myAnimeListMangaId, List<ScrappedSimpleManga> mangas);

        /// <summary>
        /// Updates all tracked Mangas (fetches new chapters or translations)
        /// </summary>
        /// <returns></returns>
        Task UpdateMangasChapters();

    }
}
