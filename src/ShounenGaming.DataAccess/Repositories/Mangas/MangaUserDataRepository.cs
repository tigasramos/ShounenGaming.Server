using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;

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

        public async Task<List<MangaUserData>> GetMangasByStatusByUser(MangaUserStatusEnum status, int userId)
        {
            return await dbSet.Where(c => c.Status == status && c.User.Id == userId).ToListAsync();
        }

        public async Task<List<MangaUserData>> GetByUser(int userId)
        {
            return await dbSet.Where(m => m.User.Id == userId).ToListAsync();
        }

        public async Task<MangaUserData?> GetByUserAndManga(int userId, int mangaId)
        {
            return await dbSet.FirstOrDefaultAsync(m => m.User.Id == userId && m.Manga.Id == mangaId);
        }

        public async Task<List<MangaUserData>> GetUsersByStatusByManga(int mangaId, MangaUserStatusEnum status)
        {
            return await dbSet.Where(m => m.Manga.Id == mangaId && m.Status == status).ToListAsync();
        }
    }
}