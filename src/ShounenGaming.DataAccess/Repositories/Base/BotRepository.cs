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
    public class BotRepository : BaseRepository<Bot>, IBotRepository
    {
        public BotRepository(DbContext context) : base(context)
        {
        }
        public async Task<Bot?> GetBotByDiscordId(string discordId)
        {
            return await dbSet.Where(u => u.DiscordId == discordId).FirstOrDefaultAsync();
        }

        public async Task<Bot?> GetBotByRefreshToken(string refreshToken)
        {
            return await dbSet.Where(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();
        }
    }
}
