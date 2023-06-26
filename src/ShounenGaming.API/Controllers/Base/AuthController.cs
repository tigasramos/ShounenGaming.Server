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
            var response = _authService.LoginBot(discordId, password);
            return Ok(response);
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
        /// Gets the unregistered Server Members
        /// </summary>
        /// <returns></returns>
        [HttpGet("members")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetNotRegisteredServerMembers()
        {
            return Ok(await _authService.GetNotRegisteredServerMembers());
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
            var response = await _authService.RefreshToken(refreshToken);
            return Ok(response);
        }

    }
}
