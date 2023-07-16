using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Base
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserByUsername(string username);
        Task<User?> GetUserByDiscordId(string discordId);
        Task<User?> GetUserByRefreshToken(string refreshToken);
    }
}
