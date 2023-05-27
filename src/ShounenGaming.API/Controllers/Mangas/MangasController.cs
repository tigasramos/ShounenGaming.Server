using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas.Enums;

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
        /// Searches a Manga (by Name or by some tags)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchManga([FromQuery] string? name, [FromQuery] List<string>? tags)
        {
            if (name is not null) return Ok(await _service.SearchMangasByName(name));
            if (tags is not null && tags.Any()) return Ok(await _service.SearchMangasByTags(tags));

            return Ok(await _service.SearchMangas());
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
        /// Search Manga from MyAnimeList
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("search/myanimelist")]
        public async Task<IActionResult> SearchMangaMetaData([FromQuery]string name)
        {
            var metadata = await _service.SearchMangaMetaData(name);
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
        /// Gets All Mangas From a Source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpGet("search/source/{source}")]
        public async Task<IActionResult> GetAllMangasFromSource(MangaSourceEnumDTO source)
        {
            var mangas = await _service.GetAllMangasFromSource(source);
            return Ok(mangas);
        }

        /// <summary>
        /// Add the Manga from MyAnimeList with that ID and links it with 
        /// </summary>
        /// <param name="myAnimeListMangaId"></param>
        /// <param name="mangas"></param>
        /// <returns></returns>
        [HttpPut("{myAnimeListMangaId}/links")]
        public async Task<IActionResult> LinkSourceToManga(int myAnimeListMangaId, [FromBody]List<ScrappedSimpleManga> mangas)
        {
            var manga = await _service.LinkSourcesToManga(myAnimeListMangaId, mangas);
            return Ok(manga);
        }

        #endregion

        [HttpPut("testing/update")]
        public async Task<IActionResult> Update()
        {
            await _service.UpdateMangasChapters();
            return Ok();
        }
    }
}
