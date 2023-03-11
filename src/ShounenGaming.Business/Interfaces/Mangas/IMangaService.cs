using ShounenGaming.Business.Models.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Business.Models.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas
{
    public interface IMangaService
    {
        Task<MangaDTO> GetMangaById(int id);
        Task<List<MangaDTO>> SearchMangaByName(string name);
        Task<List<MangaInfoDTO>> GetPopularMangas();
        Task<List<MangaInfoDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status );
        Task<List<MangaInfoDTO>> GetRecentlyAddedMangas();
        Task<List<ChapterReleaseDTO>> GetRecentlyReleasedChapters();
        Task<List<MangaWriterDTO>> GetMangaWriters();
        Task<List<string>> GetMangaTags();


        /*

        Task ChapterReadByUser(int chapterId, int userId);
        Task UpdateMangaStatusByUser(int mangaId, int userId, MangaUserStatusEnumDTO status );
        Task GetMangaUserData(int userId, int mangaId);

        //Task<List<MangaUserHistoryDTO>> GetUserHistory(int userId);
           

        Task UpdateManga();*/
        Task FetchMangas();
        
    }
}
