using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.DTOs.Models.Base;
using static ShounenGaming.Common.ExceptionMiddleware;

namespace ShounenGaming.API.Controllers.Base
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Logins a Bot
        /// </summary>
        /// <param name="discordId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet("bot/login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> LoginBot([FromQuery]string discordId, [FromQuery]string password)
        {
            var response = await _authService.LoginBot(discordId, password);
            return Ok(response);
        }

        /// <summary>
        /// Creates a Bot
        /// </summary>
        /// <param name="createBot"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")] TODO: Remove after testing
        [HttpPost("bot")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> RegisterBot(CreateBot createBot)
        {
            await _authService.RegisterBot(createBot);
            return Ok();
        }

        /// <summary>
        /// Creates a User
        /// </summary>
        /// <param name="createUser"></param>
        /// <returns></returns>
        [HttpPost("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> RegisterUser([FromBody]CreateUser createUser)
        {
            await _authService.RegisterUser(createUser);
            return Ok();
        }

        /// <summary>
        /// Creates a Login Token for that User
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost("user/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> RequestEntryToken([FromQuery]string username)
        {
            await _authService.RequestEntryToken(username);
            return Ok();
        }

        /// <summary>
        /// Logins a User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("user/login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> LoginUser([FromQuery] string username, [FromQuery] string token)
        {
            var response = await _authService.LoginUser(username, token);
            return Ok(response);
        }

        /// <summary>
        /// Refresh the Access Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [HttpGet("refreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> RefreshToken([FromQuery] string refreshToken)
        {
            var response = await _authService.RefreshToken(refreshToken, User.HasClaim("Role", "Bot"));
            return Ok(response);
        }

        /// <summary>
        /// Returns the Users that are in the Discord Server but do not have account created
        /// </summary>
        /// <returns></returns>
        [HttpGet("user/unregistered")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DiscordUserDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetUnregisteredDiscordUsers()
        {
            var response = await _authService.GetUnregisteredUsers();
            return Ok(response);
        }
    }
}
