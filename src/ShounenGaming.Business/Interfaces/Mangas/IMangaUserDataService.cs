using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas
{
    public interface IMangaUserDataService
    {
        /// <summary>
        /// Gets the Users List for some Status (eg. Completed, Reading..)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<List<MangaInfoDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status);

        /// <summary>
        /// Gets the Manga info relative to a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mangaId"></param>
        /// <returns></returns>
        Task<MangaUserDataDTO?> GetMangaDataByMangaByUser(int userId, int mangaId);

        /// <summary>
        /// Gets the User Status update history
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //Task<List<MangaUserHistoryDTO>> GetUserHistory(int userId);

        /// <summary>
        /// Marks a Chapter to be read by a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        Task MarkChapterRead(int userId, int chapterId);

        /// <summary>
        /// Unmarks a Chapter that was read by a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        Task UnmarkChapterRead(int userId, int chapterId);
        
        /// <summary>
        /// Change Manga Status relative to a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mangaId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task UpdateMangaStatusByUser(int userId, int mangaId, MangaUserStatusEnumDTO status);
       
    }
}
