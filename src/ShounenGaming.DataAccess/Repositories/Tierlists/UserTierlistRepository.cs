using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Tierlists;
using ShounenGaming.DataAccess.Interfaces.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Tierlists
{
    public class UserTierlistRepository : BaseRepository<UserTierlist>, IUserTierlistRepository
    {
        public UserTierlistRepository(DbContext context) : base(context)
        {
        }

        public override void DeleteDependencies(UserTierlist entity)
        {
            return;
        }

        public async Task<List<UserTierlist>> GetUserTierlistsByTierlistId(int tierlistId)
        {
            return await dbSet.Where(u => u.TierlistId == tierlistId).ToListAsync();
        }

        public async Task<List<UserTierlist>> GetUserTierlistsByUserId(int userId)
        {
            return await dbSet.Where(u => u.UserId == userId).ToListAsync();
        }
    }
}
