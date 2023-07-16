using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Tierlists;
using ShounenGaming.DTOs.Models.Tierlists;
using ShounenGaming.DTOs.Models.Tierlists.Requests;
using static ShounenGaming.Common.ExceptionMiddleware;

namespace ShounenGaming.API.Controllers.Tierlists
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TierlistsController : ControllerBase
    {
        private readonly ITierlistService _tierlistService;

        public TierlistsController(ITierlistService tierlistService)
        {
            _tierlistService = tierlistService;
        }
        #region Tierlist
        /// <summary>
        /// Gets Tierlist by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetTierlistById(int id)
        {
            var tierlist = await _tierlistService.GetTierlistById(id);
            return Ok(tierlist);
        }

        /// <summary>
        /// Creates a Tierlist
        /// </summary>
        /// <param name="createTierlist"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> CreateTierlist(CreateTierlist createTierlist)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var tierlist = await _tierlistService.CreateTierlist(createTierlist, userId);
            return Ok(tierlist);
        }

        /// <summary>
        /// Edits a Tierlist
        /// </summary>
        /// <param name="editTierlist"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> EditTierlist(EditTierlist editTierlist)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var tierlist = await _tierlistService.EditTierlist(editTierlist, userId);
            return Ok(tierlist);
        }
        
        /// <summary>
        /// Adds a Default Tier to a Tierlist
        /// </summary>
        /// <param name="tierlistId"></param>
        /// <param name="tier"></param>
        /// <returns></returns>
        [HttpPost("{tierlistId}/tiers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> AddDefaultTier(int tierlistId, CreateTier tier)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var tierlist = await _tierlistService.AddDefaultTier(tierlistId, tier, userId);
            return Ok(tierlist);
        }

        /// <summary>
        /// Edits a Default Tier from a Tierlist
        /// </summary>
        /// <param name="tierlistId"></param>
        /// <param name="tier"></param>
        /// <returns></returns>
        [HttpPut("{tierlistId}/tiers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> EditDefaultTier(int tierlistId, EditTier tier)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var tierlist = await _tierlistService.EditDefaultTier(tierlistId, tier, userId);
            return Ok(tierlist);
        }

        /// <summary>
        /// Deletes a Default Tier
        /// </summary>
        /// <param name="tierlistId"></param>
        /// <param name="tierId"></param>
        /// <returns></returns>
        [HttpDelete("{tierlistId}/tiers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> DeleteDefaultTier(int tierlistId, int tierId)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var tierlist = await _tierlistService.RemoveDefaultTier(tierlistId, tierId, userId);
            return Ok(tierlist);
        }

        /// <summary>
        /// Adds a Tierlist Item
        /// </summary>
        /// <param name="tierlistId"></param>
        /// <param name="tierlistItem"></param>
        /// <returns></returns>
        [HttpPost("{tierlistId}/items")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> AddTierlistItem(int tierlistId, CreateTierlistItem tierlistItem)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var tierlist = await _tierlistService.AddTierlistItem(tierlistId, tierlistItem, userId);
            return Ok(tierlist);
        }
        #endregion

        #region User

        /// <summary>
        /// Creates a User Tierlist
        /// </summary>
        /// <param name="userTierlist"></param>
        /// <returns></returns>
        [HttpPost("users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> CreateUserTierlist(CreateUserTierlist userTierlist)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var userTierlists = await _tierlistService.CreateUserTierlist(userTierlist, userId);
            return Ok(userTierlists);
        }
        
        /// <summary>
        /// Edits a UserTierlist
        /// </summary>
        /// <param name="userTierlist"></param>
        /// <returns></returns>
        [HttpPut("users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> EditUserTierlist(EditUserTierlist userTierlist)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var userTierlists = await _tierlistService.EditUserTierlist(userTierlist, userId);
            return Ok(userTierlists);
        }

        /// <summary>
        /// Adds a Tier to a UserTierlist
        /// </summary>
        /// <param name="userTierlistId"></param>
        /// <param name="tier"></param>
        /// <returns></returns>
        [HttpPost("users/{userTierlistId}/tier")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> AddTierToUserTierlist(int userTierlistId, CreateTier tier)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var userTierlists = await _tierlistService.AddTierToUserTierlist(userTierlistId, tier, userId);
            return Ok(userTierlists);
        }

        /// <summary>
        /// Edits a Tier from a UserTierlist
        /// </summary>
        /// <param name="userTierlistId"></param>
        /// <param name="tier"></param>
        /// <returns></returns>
        [HttpPut("users/{userTierlistId}/tier")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> EditTierFromUserTierlist(int userTierlistId, EditTier tier)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var userTierlists = await _tierlistService.EditTierFromUserTierlist(userTierlistId, tier, userId);
            return Ok(userTierlists);
        }

        /// <summary>
        /// Deletes a Tier from a UserTierlist
        /// </summary>
        /// <param name="userTierlistId"></param>
        /// <param name="tierId"></param>
        /// <returns></returns>
        [HttpDelete("users/{userTierlistId}/tier")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> RemoveTierFromUserTierlist(int userTierlistId, int tierId)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            var userTierlists = await _tierlistService.RemoveTierFromUserTierlist(userTierlistId, tierId, userId);
            return Ok(userTierlists);
        }
        
        /// <summary>
        /// Gets Users Tierlists
        /// </summary>
        /// <returns></returns>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetUserTierlists()
        {
            var userTierlists = await _tierlistService.GetUserTierlists();
            return Ok(userTierlists);
        }

        /// <summary>
        /// Gets All Users Tierlists by a User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetUserTierlistsByUserId(int userId)
        {
            var userTierlists = await _tierlistService.GetUserTierlistsByUserId(userId);
            return Ok(userTierlists);
        }

        /// <summary>
        /// Gets All Users Tierlists based on a Tierlist
        /// </summary>
        /// <param name="tierlistId"></param>
        /// <returns></returns>
        [HttpGet("users/tierlists/{tierlistId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserTierlistDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetUserTierlistsByTierlistId(int tierlistId)
        {
            var userTierlists = await _tierlistService.GetUserTierlistsByTierlistId(tierlistId);
            return Ok(userTierlists);
        }

        #endregion

        #region Category

        /// <summary>
        /// Gets all Tierlist Categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TierlistCategoryDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetTierlistCategories()
        {
            var categories = await _tierlistService.GetTierlistCategories();
            return Ok(categories);
        }

        /// <summary>
        /// Creates a Tierlist Category
        /// </summary>
        /// <param name="tierListCategory"></param>
        /// <returns></returns>
        [HttpPost("categories")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TierlistCategoryDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> CreateTierlistCategory(CreateTierlistCategory tierListCategory)
        {
            var category = await _tierlistService.CreateTierlistCategory(tierListCategory);
            return Ok(category);
        }

        #endregion
    }
}
