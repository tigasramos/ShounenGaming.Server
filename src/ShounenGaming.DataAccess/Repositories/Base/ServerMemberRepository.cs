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
    public class ServerMemberRepository : BaseRepository<ServerMember>, IServerMemberRepository
    {
        public ServerMemberRepository(DbContext context) : base(context)
        {
        }


        public async Task<ServerMember?> GetMemberByDiscordId(string discordId)
        {
            return await dbSet.Where(u => u.DiscordId == discordId).FirstOrDefaultAsync();
        }

        public async Task<List<ServerMember>> GetUnregisteredServerMembers()
        {
            return await dbSet.Where(s => s.User == null).ToListAsync();
        }
    }
}
