using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities;
using ShounenGaming.DataAccess.Interfaces;

namespace ShounenGaming.DataAccess.Repositories
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : SimpleEntity
    {
        protected readonly DbContext context;
        protected readonly DbSet<TEntity> dbSet;

        protected BaseRepository(DbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity> Create(TEntity entity)
        {
            var addedEntity = (await dbSet.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();

            return addedEntity;
        }

        public async Task<bool> Delete(int id)
        {
            var entityToRemove = await dbSet.FirstOrDefaultAsync(x => x.Id == id);
            if (entityToRemove == null)
                return false;

            DeleteDependencies(entityToRemove);

            dbSet.Remove(entityToRemove);
            await context.SaveChangesAsync();
            return true;
        }

        public virtual void DeleteDependencies(TEntity entity) { }

        public async Task<IList<TEntity>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<TEntity?> GetById(int id)
        {
            return await dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<TEntity> Update(TEntity entity)
        {
            var updatedEntity = dbSet.Update(entity).Entity;
            await context.SaveChangesAsync();

            return updatedEntity;
        }
    }
}
