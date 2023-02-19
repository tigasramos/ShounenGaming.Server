using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Tierlists;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DataAccess.Interfaces.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{
    public class MangaRepository : BaseRepository<Manga>, IMangaRepository
    {
        public MangaRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> Delete(int id)
        {
            var entityToRemove = await dbSet.FirstOrDefaultAsync(x => x.Id == id);
            if (entityToRemove == null)
                return false;

            context.RemoveRange(entityToRemove.AlternativeNames);

            foreach(var chapter in entityToRemove.Chapters)
            {
                foreach (var translation in chapter.Translations)
                {
                    foreach(var page in translation.Pages)
                    {
                        context.Remove(page.Image);
                    }
                    context.RemoveRange(translation.Pages);
                }
                context.RemoveRange(chapter.Translations);
            }
            context.RemoveRange(entityToRemove.Chapters);

            dbSet.Remove(entityToRemove);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
