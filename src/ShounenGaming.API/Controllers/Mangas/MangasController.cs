using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.Business.Helpers;
using ShounenGaming.DTOs.Models.Mangas;

namespace ShounenGaming.API.Controllers.Mangas
{
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
            var mangaTranslation = await _service.GetMangaTranslation(mangaId, chapterId, translation);
            return Ok(mangaTranslation);
        }


        /// <summary>
        /// Searches a Manga (by Name or by some tags)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchManga([FromBody]SearchMangaQueryDTO query, [FromQuery] int page = 1)
        {
            var mangas = await _service.SearchMangas(query, page);
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
        /// Gets the last Mangas added
        /// </summary>
        /// <returns></returns>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentlyAddedMangas()
        {
            var mangas = await _service.GetRecentlyAddedMangas();
            return Ok(mangas);
        }

        /// <summary>
        /// Gets the last Chapters added
        /// </summary>
        /// <returns></returns>
        [HttpGet("recent/chapters")]
        public async Task<IActionResult> GetRecentlyReleasedChapters()
        {
            var chapters = await _service.GetRecentlyReleasedChapters();
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
            //TODO: Put UserId
            var metadata = await _service.AddManga(source, mangaId, 1);
            return Ok(metadata);
        }

        /// <summary>
        /// Search Manga from either MyAnimetList or AniList
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("search/{source}")]
        public async Task<IActionResult> SearchMangaMetadata(MangaMetadataSourceEnumDTO source, [FromQuery]string name)
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
        public async Task<IActionResult> SearchMangaSources([FromQuery]string name)
        {
            var mangas = await _service.SearchMangaSource(name);
            return Ok(mangas);
        }

        /// <summary>
        /// Gets All Mangas From a Source by Page
        /// </summary>
        /// <param name="source"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet("search/source/{source}")]
        public async Task<IActionResult> GetAllMangasFromSource(MangaSourceEnumDTO source, [FromQuery]int page = 1)
        {
            var mangas = await _service.GetAllMangasFromSourceByPage(source, page);
            return Ok(mangas);
        }

        /// <summary>
        /// Adds Sources to an already existing Manga with that Id
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="mangas"></param>
        /// <returns></returns>
        [HttpPut("{mangaId}/links")]
        public async Task<IActionResult> LinkSourceToManga(int mangaId, [FromBody]List<MangaSourceDTO> mangas)
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
            await _service.StartMangaChaptersUpdate(mangaId);
            return Ok();
        }

        #endregion

    }
}
