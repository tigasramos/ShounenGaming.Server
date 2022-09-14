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
    public class TierlistCategoryRepository : BaseRepository<TierlistCategory>, ITierlistCategoryRepository
    {
        public TierlistCategoryRepository(DbContext context) : base(context)
        {
        }
    }
}
