using ShounenGaming.Core.Entities.Mangas;

namespace ShounenGaming.DataAccess.Interfaces.Mangas
{
    public interface IChangedMangaStatusActionRepository : IBaseRepository<ChangedMangaStatusAction>
    {
        Task<ChangedMangaStatusAction?> GetLastMangaUserStatusUpdate(int userId, int mangaId);

        Task<List<ChangedMangaStatusAction>> GetLastN(int count);
    }
}
