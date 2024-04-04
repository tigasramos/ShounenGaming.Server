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

    public class MangaTranslationRepository : BaseRepository<MangaTranslation>, IMangaTranslationRepository
    {
        public MangaTranslationRepository(DbContext context) : base(context)
        {
        }
    }
}
