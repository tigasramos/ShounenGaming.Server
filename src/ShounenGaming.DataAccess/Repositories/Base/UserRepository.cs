using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.DataAccess.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Base
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }


        public async Task<User?> GetUserByDiscordId(string discordId)
        {
            return await dbSet.Where(u => u.ServerMember != null && u.ServerMember.DiscordId == discordId).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByRefreshToken(string refreshToken)
        {
            return await dbSet.Where(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await dbSet.Where(u => u.Username == username).FirstOrDefaultAsync();
        }
    }
}
