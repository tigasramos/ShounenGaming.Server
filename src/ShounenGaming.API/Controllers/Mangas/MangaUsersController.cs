using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.API.Controllers.Mangas
{
    [Route("api/mangas")]
    [ApiController]
    public class MangaUsersController : ControllerBase
    {
        private readonly IMangaUserDataService _mangaUsersService;

        public MangaUsersController(IMangaUserDataService mangaUsersService)
        {
            _mangaUsersService = mangaUsersService;
        }

        /// <summary>
        /// Gets Manga Data from a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        [HttpGet("{mangaId}/user/{userId}")]
        public async Task<IActionResult> GetMangaDataByMangaByUser(int userId, int mangaId)
        {
            var data = await _mangaUsersService.GetMangaDataByMangaByUser(userId, mangaId);
            return Ok(data);
        }

        /// <summary>
        /// Gets all Mangas from a User library in a certain Status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet("user/{userId}/status/{status}")]
        public async Task<IActionResult> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status)
        {
            var data = await _mangaUsersService.GetMangasByStatusByUser(userId, status);
            return Ok(data);
        }

        /// <summary>
        /// Marks a Chapter as read for the current User
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        [HttpPut("read/{chapterId}")]
        public async Task<IActionResult> MarkChapterRead(int chapterId)
        {
            //TODO: Get User Id from JWT Token
            return Ok(await _mangaUsersService.MarkChapterRead(1, chapterId));
        }

        /// <summary>
        /// Unmarks a Chapter as read for the current User
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        [HttpPut("unread/{chapterId}")]
        public async Task<IActionResult> UnmarkChapterRead(int chapterId)
        {
            //TODO: Get User Id from JWT Token
            return Ok(await _mangaUsersService.UnmarkChapterRead(1, chapterId));
        }

        /// <summary>
        /// Updates the Status for the Manga to the current User
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPut("{mangaId}/status")]
        public async Task<IActionResult> UpdateMangaStatusByUser(int mangaId, [FromQuery]MangaUserStatusEnumDTO? status = null)
        {
            //TODO: Get User Id from JWT Token
            return Ok(await _mangaUsersService.UpdateMangaStatusByUser(1, mangaId, status));
        }
    }
}
