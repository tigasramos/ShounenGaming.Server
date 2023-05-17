using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Interfaces.Mangas;

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


        [HttpGet("{mangaId}/user/{userId}")]
        public async Task<IActionResult> GetDataByMangaAndUser(int mangaId, int userId)
        {
            var data = await _mangaUsersService.GetMangaDataByUserAndManga(userId, mangaId);
            return Ok(data);
        }
    }
}
