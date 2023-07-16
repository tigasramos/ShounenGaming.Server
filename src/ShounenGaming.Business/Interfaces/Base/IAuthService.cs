
using ShounenGaming.Core.Entities.Base.Enums;
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
        Task<List<ServerMemberDTO>> GetNotRegisteredServerMembers();
        Task RegisterUser(CreateUser createUser);
        Task RequestEntryToken(string username);
        Task<AuthResponse> LoginUser(string username, string token);
        AuthResponse LoginBot(string discordId, string password);
        Task<AuthResponse> RefreshToken(string refreshToken);
        Task AcceptAccountVerification(string discordId);
        Task RejectAccountVerification(string discordId);

        Task UpdateServerMember(string discordId, string discordImageUrl, string displayName, string username, RolesEnum? role);

    }
}
