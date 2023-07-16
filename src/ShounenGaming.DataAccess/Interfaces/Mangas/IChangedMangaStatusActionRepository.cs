using ShounenGaming.Core.Entities.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Mangas
{
    public interface IChangedMangaStatusActionRepository : IBaseRepository<ChangedMangaStatusAction>
    {
        Task<ChangedMangaStatusAction?> GetLastMangaUserStatusUpdate(int userId, int mangaId);
    }
}
