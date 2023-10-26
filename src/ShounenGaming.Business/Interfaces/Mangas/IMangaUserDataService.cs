using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;

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
        Task<List<MangaUserDataDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status);

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
        Task<MangaUserDataDTO> MarkChaptersRead(int userId, List<int> chaptersIds);

        /// <summary>
        /// Unmarks a Chapter that was read by a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        Task<MangaUserDataDTO?> UnmarkChaptersRead(int userId, List<int> chaptersIds);
        
        /// <summary>
        /// Change Manga Status relative to a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mangaId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<MangaUserDataDTO?> UpdateMangaStatusByUser(int userId, int mangaId, MangaUserStatusEnumDTO? status);

        /// <summary>
        /// Change Manga Rating relative to a User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mangaId"></param>
        /// <param name="rating"></param>
        /// <returns></returns>
        Task<MangaUserDataDTO?> UpdateMangaRatingByUser(int userId, int mangaId, double? rating);


        /// <summary>
        /// Gets list of last Users activities
        /// </summary>
        /// <returns></returns>
        Task<List<MangasUserActivityDTO>> GetLastUsersActivity();
    }
}
