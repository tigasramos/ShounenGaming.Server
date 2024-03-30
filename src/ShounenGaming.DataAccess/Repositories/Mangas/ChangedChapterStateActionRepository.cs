using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{
    public class ChangedChapterStateActionRepository : BaseRepository<ChangedChapterStateAction>, IChangedChapterStateActionRepository
    {
        public ChangedChapterStateActionRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<ChangedChapterStateAction>> GetChapterHistoryFromUser(int userId)
        {
            return await dbSet.Where(c => c.UserId == userId).ToListAsync();
        }

        public async Task<ChangedChapterStateAction?> GetFirstChapterUserReadFromManga(int userId, int mangaId)
        {
            return await dbSet.Where(h => h.UserId == userId && h.Chapter.MangaId == mangaId).OrderBy(o => o.CreatedAt).FirstOrDefaultAsync();
        }

        public async Task<ChangedChapterStateAction?> GetLastChapterUserReadFromManga(int userId, int mangaId)
        {
            return await dbSet.Where(h => h.UserId == userId && h.Chapter.MangaId == mangaId).OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        }
        public async Task<List<ChangedChapterStateAction>> GetLastN(int count)
        {
            return await dbSet.OrderByDescending(c => c.CreatedAt).Take(count).ToListAsync();
        }
    }
}
