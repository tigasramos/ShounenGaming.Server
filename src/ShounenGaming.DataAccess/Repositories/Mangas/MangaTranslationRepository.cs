using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;
namespace ShounenGaming.DataAccess.Repositories.Mangas
{

    public class MangaTranslationRepository : BaseRepository<MangaTranslation>, IMangaTranslationRepository
    {
        public MangaTranslationRepository(DbContext context) : base(context)
        {
        }
    }
}
