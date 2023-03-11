using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{
    public class MangaUserDataRepository : BaseRepository<MangaUserData>, IMangaUserDataRepository
    {
        public MangaUserDataRepository(DbContext context) : base(context)
        {
        }

        public override void DeleteDependencies(MangaUserData data)
        {
            context.RemoveRange(data.ChaptersRead);
        }

        public async Task<List<MangaUserData>> GetByStatusByUser(MangaUserStatusEnum status, int userId)
        {
            return await dbSet.Where(c => c.Status == status && c.User.Id == userId).ToListAsync();
        }
    }
}