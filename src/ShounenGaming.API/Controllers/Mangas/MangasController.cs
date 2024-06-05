using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.DTOs.Models.Mangas;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ShounenGaming.Business.Helpers;

namespace ShounenGaming.API.Controllers.Mangas
{
    [Authorize]
    [Route("api/mangas")]
    [ApiController]
    public class MangasController : ControllerBase
    {
        private readonly IMangaService _service;

        public MangasController(IMangaService service)
        {
            _service = service;
        }

        #region Get Mangas

        /// <summary>
        /// Gets a Manga by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manga = await _service.GetMangaById(id);
            return Ok(manga);
        }

        /// <summary>
        /// Gets the Manga Sources by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/sources")]
        public async Task<IActionResult> GetSourcesById(int id)
        {
            var sources = await _service.GetMangaSourcesById(id);
            return Ok(sources);
        }

        /// <summary>
        /// Gets a Manga Translation by its Id
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="chapterId"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        [HttpGet("{mangaId}/chapters/{chapterId}/translations/{translation}")]
        public async Task<IActionResult> GetMangaTranslation(int mangaId, int chapterId, MangaTranslationEnumDTO translation)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var mangaTranslation = await _service.GetMangaTranslation(userId, mangaId, chapterId, translation);
            return Ok(mangaTranslation);
        }

        /// <summary>
        /// Gets the Mangas from the Season Animes
        /// </summary>
        /// <returns></returns>
        [HttpGet("season")]
        public async Task<IActionResult> GetSeasonMangas()
        {
            var mangas = await _service.GetSeasonMangas();
            return Ok(mangas);
        }

        /// <summary>
        /// Searches a Manga (by Name or by some tags)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchManga([FromQuery] int page = 1, [FromQuery] string? name = null)
        {
            var userId = User.FindFirstValue("Id");
            var mangas = await _service.SearchMangas(new SearchMangaQueryDTO { Name = name }, page, userId != null ? Convert.ToInt32(userId) : null);
            return Ok(mangas);
        }

        /// <summary>
        /// Gets Mangas waiting for Sources
        /// </summary>
        /// <returns></returns>
        [Authorize(policy: "Mod")]
        [HttpGet("waiting")]
        public async Task<IActionResult> GetWaitingMangas()
        {
            var mangas = await _service.GetWaitingMangas();
            return Ok(mangas);
        }

        /// <summary>
        /// Gets the most Popular Mangas at the current time
        /// </summary>
        /// <returns></returns>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var popularMangas = await _service.GetPopularMangas();
            return Ok(popularMangas);
        }

        /// <summary>
        /// Gets the Last Mangas added
        /// </summary>
        /// <returns></returns>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentlyAddedMangas()
        {
            var mangas = await _service.GetRecentlyAddedMangas();
            return Ok(mangas);
        }

        /// <summary>
        /// Gets the Last Chapters added
        /// </summary>
        /// <returns></returns>
        [HttpGet("recent/chapters")]
        public async Task<IActionResult> GetRecentlyReleasedChapters()
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var chapters = await _service.GetRecentlyReleasedChapters(userId);
            return Ok(chapters);
        }

        #endregion

        #region Tags & Writers

        /// <summary>
        /// Gets all Manga Tags
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags")]
        public async Task<IActionResult> GetMangaTags()
        {
            var tags = await _service.GetMangaTags();
            return Ok(tags);
        }

        /// <summary>
        /// Gets the Mangas from Tag
        /// </summary>
        /// <returns></returns>
        [HttpGet("tags/{tag}")]
        public async Task<IActionResult> GetMangasFromTag(string tag)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var mangas = await _service.GetMangasFromTag(tag, userId);
            return Ok(mangas);
        }

        /// <summary>
        /// Gets all Manga Writers
        /// </summary>
        /// <returns></returns>
        [HttpGet("writers")]
        public async Task<IActionResult> GetMangaWriters()
        {
            var writers = await _service.GetMangaWriters();
            return Ok(writers);
        }

        /// <summary>
        /// Gets the Manga Writer by its Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("writers/{id}")]
        public async Task<IActionResult> GetMangaWriterById(int id)
        {
            var writer = await _service.GetMangaWriterById(id);
            return Ok(writer);
        }

        #endregion

        #region Manage Mangas

        /// <summary>
        /// Adds the Manga 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        [HttpPost("{source}/{mangaId}")]
        public async Task<IActionResult> AddManga(MangaMetadataSourceEnumDTO source, long mangaId)
        {
            var userId = User.FindFirstValue("Id");
            var manga = await _service.AddManga(source, mangaId, Convert.ToInt32(userId));
            return Ok(manga);
        }

        /// <summary>
        /// Search Manga from either MyAnimetList or AniList
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("search/{source}")]
        public async Task<IActionResult> SearchMangaMetadata(MangaMetadataSourceEnumDTO source, [FromQuery] string name)
        {
            var metadata = await _service.SearchMangaMetadata(source, name);
            return Ok(metadata);
        }

        /// <summary>
        /// Searches a Manga by Name in all available Sources
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("search/sources")]
        public async Task<IActionResult> SearchMangaSources([FromQuery] string name)
        {
            var mangas = await _service.SearchMangaSource(name);
            return Ok(mangas);
        }

        /// <summary>
        /// Adds Sources to an already existing Manga with that Id
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="mangas"></param>
        /// <returns></returns>
        [HttpPut("{mangaId}/links")]
        public async Task<IActionResult> LinkSourceToManga(int mangaId, [FromBody] List<MangaSourceDTO> mangas)
        {
            var manga = await _service.LinkSourcesToManga(mangaId, mangas);
            return Ok(manga);
        }

        /// <summary>
        /// Updates the Manga to get the Chapters
        /// </summary>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        [HttpPut("{mangaId}/chapters")]
        public async Task<IActionResult> FetchChaptersForManga(int mangaId)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            await _service.StartMangaChaptersUpdate(mangaId, userId);
            return Ok();
        }

        /// <summary>
        /// Gets the Fetched Mangas Chapters Queue
        /// </summary>
        /// <returns></returns>
        [HttpGet("queue")]
        public async Task<IActionResult> GetQueueStatus()
        {
            var queue = await _service.GetQueueStatus();
            return Ok(queue);
        }

        #endregion

        #region Recommendations
        /// <summary>
        /// Gets Manga Recommendations for specific User
        /// </summary>
        /// <returns></returns>
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetMangaRecommendations()
        {
            var userId = User.FindFirstValue("Id");
            var recommendations = await _service.GetMangaRecommendations(Convert.ToInt32(userId));
            return Ok(recommendations);
        }

        /// <summary>
        /// Gets Manga Recommendations for specific User
        /// </summary>
        /// <returns></returns>
        [HttpGet("recommendations/search")]
        public async Task<IActionResult> SearchMangaRecommendations()
        {
            var userId = User.FindFirstValue("Id");
            var recommendations = await _service.SearchMangaRecommendations(Convert.ToInt32(userId));
            return Ok(recommendations);
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> DeleteDuplicated()
        {
            await _service.FixDuplicatedChapters();
            return Ok();
        }

        /// <summary>
        /// Starts downloading all images for current manga
        /// </summary>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        [HttpPatch("{mangaId}/download")]
        public async Task<IActionResult> DownloadImagesFromManga(int mangaId)
        {
            await _service.DownloadImagesFromManga(mangaId);
            return Ok();
        }

        /// <summary>
        /// Starts download all images for current chapter and forces replacement
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        [HttpPatch("chapter/download/{chapterId}")]
        public async Task<IActionResult> DownloadImagesFromMangaChapter(int chapterId)
        {
            await _service.DownloadImagesFromMangaChapter(chapterId);
            return Ok();
        }
    }
}
