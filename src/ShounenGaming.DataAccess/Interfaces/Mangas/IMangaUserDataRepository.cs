using ShounenGaming.Core.Entities.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Mangas
{

    public interface IMangaUserDataRepository : IBaseRepository<MangaUserData>
    {
        Task<List<MangaUserData>> GetByUser(int userId);
        Task<MangaUserData?> GetByUserAndManga(int userId, int mangaId);
        Task<List<MangaUserData>> GetByStatusByUser(MangaUserStatusEnum status,int userId);
    }
}
