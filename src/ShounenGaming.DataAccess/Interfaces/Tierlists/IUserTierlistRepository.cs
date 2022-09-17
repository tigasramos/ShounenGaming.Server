using ShounenGaming.Core.Entities.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Tierlists
{
    public interface IUserTierlistRepository : IBaseRepository<UserTierlist>
    {
        Task<List<UserTierlist>> GetUserTierlistsByUserId(int userId);
        Task<List<UserTierlist>> GetUserTierlistsByTierlistId(int tierlistId);
    }
}
