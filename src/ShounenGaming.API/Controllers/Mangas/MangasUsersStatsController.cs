using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.DTOs.Models.Mangas.Stats;

namespace ShounenGaming.API.Controllers.Mangas
{
    [Route("api/mangas/user")]
    [ApiController]
    public class MangasUsersStatsController : ControllerBase
    {
        private readonly IMangaUserStatsService _mangaUserStatsService;

        public MangasUsersStatsController(IMangaUserStatsService mangaUserStatsService)
        {
            _mangaUserStatsService = mangaUserStatsService;
        }

        [HttpGet("{userId}/stats/main")]
        public async Task<IActionResult> GetUserMainStats(int userId) 
        {
            var mainStats = await _mangaUserStatsService.GetUserMainStats(userId);
            return Ok(mainStats);
        }

        [HttpGet("{userId}/stats/history")]
        public async Task<IActionResult> GetUserReadingHistory(int userId)
        {
            var history = await _mangaUserStatsService.GetUserReadingHistory(userId);
            return Ok(history);
        }
    }
    
}
