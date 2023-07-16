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
    public class ChangedChapterStateActionRepository : BaseRepository<ChangedChapterStateAction>, IChangedChapterStateActionRepository
    {
        public ChangedChapterStateActionRepository(DbContext context) : base(context)
        {
        }

        public async Task<ChangedChapterStateAction?> GetFirstChapterUserReadFromManga(int userId, int mangaId)
        {
            return await dbSet.Where(h => h.UserId == userId && h.Chapter.MangaId == mangaId).OrderBy(o => o.CreatedAt).FirstOrDefaultAsync();
        }

        public async Task<ChangedChapterStateAction?> GetLastChapterUserReadFromManga(int userId, int mangaId)
        {
            return await dbSet.Where(h => h.UserId == userId && h.Chapter.MangaId == mangaId).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        }
    }
}
