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
    public class TierlistRepository : BaseRepository<Tierlist>, ITierlistRepository
    {
        public TierlistRepository(DbContext context) : base(context)
        {
        }

        public override void DeleteDependencies(Tierlist entity)
        {
            context.RemoveRange(entity.DefaultTiers);
            context.Remove(entity.Image);
            foreach (var item in entity.Items)
            {
                context.Remove(item.Image);
            }
            context.RemoveRange(entity.Items);
        }
    }
}
