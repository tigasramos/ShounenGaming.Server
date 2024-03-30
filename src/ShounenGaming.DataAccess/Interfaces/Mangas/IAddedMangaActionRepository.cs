using ShounenGaming.Core.Entities.Mangas;

namespace ShounenGaming.DataAccess.Interfaces.Mangas
{
    public interface IAddedMangaActionRepository : IBaseRepository<AddedMangaAction>
    {
        Task<List<AddedMangaAction>> GetLastN(int count);
    }
}
