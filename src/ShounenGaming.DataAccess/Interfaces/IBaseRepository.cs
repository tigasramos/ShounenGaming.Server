using ShounenGaming.Core.Entities;

namespace ShounenGaming.DataAccess.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : SimpleEntity
    {
        Task<IList<TEntity>> GetAll();
        Task<TEntity?> GetById(int id);
        Task<TEntity> Create(TEntity entity);
        Task<TEntity> Update(TEntity entity);
        Task<bool> Delete(int id);
    }
}
