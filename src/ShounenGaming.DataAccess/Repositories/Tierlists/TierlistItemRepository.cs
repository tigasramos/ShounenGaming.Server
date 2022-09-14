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
    public class TierlistItemRepository : BaseRepository<TierlistItem>, ITierlistItemRepository
    {
        public TierlistItemRepository(DbContext context) : base(context)
        {
        }
    }
}
