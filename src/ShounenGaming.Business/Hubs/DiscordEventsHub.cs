using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Core.Entities.Base.Enums;
using ShounenGaming.DTOs.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Hubs
{
    public interface IDiscordEventsHubClient
    {
        Task SendVerifyAccount(string discordId, string fullName);
        Task SendToken(string discordId, string token, DateTime expireDate);
    }

    [Authorize(Policy = "Bot")]
    public class DiscordEventsHub : Hub<IDiscordEventsHubClient>
    {
        private readonly IAuthService _authService;

        public DiscordEventsHub(IAuthService authService)
        {
            _authService = authService;
        }

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

        public async Task UpdateServerMember(string discordId, string discordImageUrl, string displayName, string username, RolesEnum? role)
        {
            await _authService.UpdateServerMember(discordId, discordImageUrl, displayName, username, role);
        }
        
    }
}
