using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Security.Claims;

namespace ShounenGaming.API.Controllers.Mangas
{
    [Authorize]
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
        /// Marks the Chapters as read for the current User
        /// </summary>
        /// <param name="chaptersIds"></param>
        /// <returns></returns>
        [HttpPut("read")]
        public async Task<IActionResult> MarkChapterRead([FromBody] List<int> chaptersIds)
        {
            var userId = User.FindFirstValue("Id");
            return Ok(await _mangaUsersService.MarkChaptersRead(Convert.ToInt32(userId), chaptersIds));
        }

        /// <summary>
        /// Unmarks the Chapters as read for the current User
        /// </summary>
        /// <param name="chaptersIds"></param>
        /// <returns></returns>
        [HttpPut("unread")]
        public async Task<IActionResult> UnmarkChapterRead([FromBody] List<int> chaptersIds)
        {
            var userId = User.FindFirstValue("Id");
            return Ok(await _mangaUsersService.UnmarkChaptersRead(Convert.ToInt32(userId), chaptersIds));
        }

        /// <summary>
        /// Updates the Status for the Manga to the current User
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPut("{mangaId}/status")]
        public async Task<IActionResult> UpdateMangaStatusByUser(int mangaId, [FromQuery] MangaUserStatusEnumDTO? status = null)
        {
            var userId = User.FindFirstValue("Id");
            var userData = await _mangaUsersService.UpdateMangaStatusByUser(Convert.ToInt32(userId), mangaId, status);
            if (userData != null) 
                return Ok(userData);

            return Ok();
        }

        /// <summary>
        /// Updates the Rating for the Manga to the current User
        /// </summary>
        /// <param name="mangaId"></param>
        /// <param name="rating"></param>
        /// <returns></returns>
        [HttpPut("{mangaId}/rating")]
        public async Task<IActionResult> UpdateMangaStatusByUser(int mangaId, [FromQuery] double? rating = null)
        {
            var userId = User.FindFirstValue("Id");
            var userData = await _mangaUsersService.UpdateMangaRatingByUser(Convert.ToInt32(userId), mangaId, rating);
            if (userData != null)
                return Ok(userData);

            return Ok();
        }

        /// <summary>
        /// Gets Manga Recommendations for specific User
        /// </summary>
        /// <returns></returns>
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetMangaRecommendations()
        {
            var userId = User.FindFirstValue("Id");
            var userData = await _mangaUsersService.GetMangaRecommendations(Convert.ToInt32(userId));
            return Ok(userData);
        }

        /// <summary>
        /// Gets Last Community Activities
        /// </summary>
        /// <returns></returns>
        [HttpGet("activities")]
        public async Task<IActionResult> GetLastCommunityActivities()
        {
            var activity = await _mangaUsersService.GetLastUsersActivity();
            return Ok(activity);
        }
    }
}
