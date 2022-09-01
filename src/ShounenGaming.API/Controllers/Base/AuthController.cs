using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Models.Base;

namespace ShounenGaming.API.Controllers.Base
{
    [Route("api/[controller]")]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> LoginBot([FromQuery]string discordId, [FromQuery]string password)
        {
            try
            {
                var response = await _authService.LoginBot(discordId, password);
                return Ok(response);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (WrongCredentialsException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Creates a Bot
        /// </summary>
        /// <param name="createBot"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")] TODO: Remove after testing
        [HttpPost("bot")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> RegisterBot(CreateBot createBot)
        {
            try
            {
                await _authService.RegisterBot(createBot);
                return Ok();
            } 
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Creates a User
        /// </summary>
        /// <param name="createUser"></param>
        /// <returns></returns>
        [HttpPost("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> RegisterUser(CreateUser createUser)
        {
            try
            {
                await _authService.RegisterUser(createUser);
                return Ok();
            }
            catch(InvalidParameterException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Creates a Login Token for that User
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost("user/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> RequestEntryToken([FromQuery]string username)
        {
            try
            {
                await _authService.RequestEntryToken(username);
                return Ok();
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (DiscordAccountNotConfirmedException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Logins a User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("user/login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> LoginUser([FromQuery] string username, [FromQuery] string token)
        {
            try
            {
                var response = await _authService.LoginUser(username, token);
                return Ok(response);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (WrongCredentialsException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Refresh the Access Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [HttpGet("refreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> RefreshToken([FromHeader] string refreshToken)
        {
            try
            {
                var response = await _authService.RefreshToken(refreshToken, User.HasClaim("Role", "Bot"));
                return Ok(response);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Returns the Users that are in the Discord Server but do not have account created
        /// </summary>
        /// <returns></returns>
        [HttpGet("user/unregistered")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DiscordUserDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> GetUnregisteredDiscordUsers()
        {
            try
            {
                var response = await _authService.GetUnregisteredUsers();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
