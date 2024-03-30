using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{

    public class AddedMangaActionRepository : BaseRepository<AddedMangaAction>, IAddedMangaActionRepository
    {
        public AddedMangaActionRepository(DbContext context) : base(context)
        {
        }
        public async Task<List<AddedMangaAction>> GetLastN(int count)
        {
            return await dbSet.OrderByDescending(c => c.CreatedAt).Take(count).ToListAsync();
        }
    }
}
