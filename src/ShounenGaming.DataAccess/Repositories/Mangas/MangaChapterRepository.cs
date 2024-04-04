using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{

    public class MangaChapterRepository : BaseRepository<MangaChapter>, IMangaChapterRepository
    {
        public MangaChapterRepository(DbContext context) : base(context)
        {
        }
    }
}
