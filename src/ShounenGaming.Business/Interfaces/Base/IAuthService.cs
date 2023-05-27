using ShounenGaming.DTOs.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Base
{
    public interface IAuthService
    {
        Task RegisterBot(CreateBot createBot);
        Task RegisterUser(CreateUser createUser);
        Task RequestEntryToken(string username);
        Task<AuthResponse> LoginUser(string username, string token);
        Task<AuthResponse> LoginBot(string discordId, string password);
        Task<AuthResponse> RefreshToken(string refreshToken, bool isBot);
        Task<List<DiscordUserDTO>> GetUnregisteredUsers();
        Task VerifyDiscordAccount(string discordId);
        void SetDiscordUsers(List<DiscordUserDTO> users);

        //TODO: VerifyEmail
    }
}
