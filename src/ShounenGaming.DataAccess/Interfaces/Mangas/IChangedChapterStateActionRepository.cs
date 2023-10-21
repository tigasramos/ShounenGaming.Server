using ShounenGaming.Core.Entities.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Mangas
{
    public interface IChangedChapterStateActionRepository : IBaseRepository<ChangedChapterStateAction>
    {
        Task<List<ChangedChapterStateAction>> GetChapterHistoryFromUser(int userId);
        Task<ChangedChapterStateAction?> GetLastChapterUserReadFromManga(int userId, int mangaId);
        Task<ChangedChapterStateAction?> GetFirstChapterUserReadFromManga(int userId, int mangaId);
    }
}
