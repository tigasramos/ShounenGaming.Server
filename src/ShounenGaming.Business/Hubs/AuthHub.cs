using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Hubs
{
    public interface IAuthHubClient
    {
        Task SendVerifyAccount(string discordId, string fullName);
        Task SendToken(string discordId, string token, DateTime expireDate);
    }

    [Authorize]
    public class AuthHub : Hub<IAuthHubClient>
    {
        private readonly IAuthService _authService;

        public AuthHub(IAuthService authService)
        {
            _authService = authService;
        }

        [Authorize(Policy = "Bot")]
        public void UpdateDiscordUsers(List<DiscordUserDTO> users)
        {
            _authService.SetDiscordUsers(users ?? new List<DiscordUserDTO>());
        }

        [Authorize(Policy = "Bot")]
        public async Task<bool> VerifyAccount(string discordId)
        {
            try
            {
                await _authService.VerifyDiscordAccount(discordId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
