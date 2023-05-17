using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;

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

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var popularMangas = await _service.GetPopularMangas();
            return Ok(popularMangas);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manga = await _service.GetMangaById(id);
            return Ok(manga);
        }
        

        #region Manage Mangas
        /*
         * Mods Only
         * Flow: Search Manga By Name in MAL -> Search Manga By Name in Sources -> Link Manga to Sources
         */

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


        [HttpPut("testing/update")]
        public async Task<IActionResult> Update()
        {
            await _service.UpdateMangasChapters();
            return Ok();
        }
        #endregion
    }
}
