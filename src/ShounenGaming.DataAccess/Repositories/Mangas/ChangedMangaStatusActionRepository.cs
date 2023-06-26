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
    public class ChangedMangaStatusActionRepository : BaseRepository<ChangedMangaStatusAction>, IChangedMangaStatusActionRepository
    {
        public ChangedMangaStatusActionRepository(DbContext context) : base(context)
        {
        }


        public async Task<ChangedMangaStatusAction?> GetLastMangaUserStatusUpdate(int userId, int mangaId)
        {
            return await dbSet.Where(h => h.MangaId == mangaId && h.UserId == userId).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        }
    }
}
