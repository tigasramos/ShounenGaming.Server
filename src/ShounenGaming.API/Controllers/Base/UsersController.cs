﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.DTOs.Models.Base;
using static ShounenGaming.Common.ExceptionMiddleware;

namespace ShounenGaming.API.Controllers.Base
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetUsers());
        }

        /// <summary>
        /// Gets the Logged User
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetLoggedUser()
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            return Ok(await _userService.GetUserById(userId));
        }

        /// <summary>
        /// Gets a User by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetUserById(int id)
        {
            return Ok(await _userService.GetUserById(id));
        }

        /// <summary>
        /// Gets the Configs for the Mangas Module
        /// </summary>
        /// <returns></returns>
        [HttpGet("configs/mangas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserMangasConfigsDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> GetUserConfigsForMangasModule()
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            return Ok(await _userService.GetUserConfigsForMangas(userId));
        }

        /// <summary>
        /// Updates the Configs for the Mangas Module
        /// </summary>
        /// <returns></returns>
        [HttpPut("configs/mangas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserMangasConfigsDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
        public async Task<IActionResult> ChangeUserConfigsForMangasModule(ChangeUserMangasConfigsDTO userMangasConfigs)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == "Id")!.Value);
            return Ok(await _userService.ChangeUserConfigsForMangas(userId, userMangasConfigs));
        }
    }
}
