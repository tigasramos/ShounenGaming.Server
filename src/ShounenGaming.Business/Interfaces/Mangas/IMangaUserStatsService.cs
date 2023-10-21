using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.DTOs.Models.Mangas.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas
{
    public interface IMangaUserStatsService
    {
        Task<UserMangaMainStatsDTO> GetUserMainStats(int userId);
        Task<List<UserChapterReadHistoryDTO>> GetUserReadingHistory(int userId);
    }
}
