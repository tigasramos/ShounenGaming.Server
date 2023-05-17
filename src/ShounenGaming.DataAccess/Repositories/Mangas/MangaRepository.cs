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
using System.Xml.Linq;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{
    public class MangaRepository : BaseRepository<Manga>, IMangaRepository
    {
        public MangaRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Manga>> GetPopularMangas(int count = 5)
        {
            return await dbSet.OrderByDescending(m => m.UsersData.Where(c => c.Status == MangaUserStatusEnum.READING).Count()).Take(count).ToListAsync();
        }


        public override void DeleteDependencies(Manga entity)
        {
            context.RemoveRange(entity.AlternativeNames);

            foreach (var chapter in entity.Chapters)
            {
                context.RemoveRange(chapter.Translations);
            }
            context.RemoveRange(entity.Chapters);
        }

        public async Task<List<Manga>> SearchMangaByName(string name)
        {
            return await dbSet.Where(m => m.Name.Contains(name) || m.AlternativeNames.Any(d => d.Name.Contains(name))).ToListAsync();
        }

        public async Task<List<Manga>> GetRecentlyAddedMangas()
        {
            return await dbSet.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }

        public async Task<List<MangaChapter>> GetRecentlyReleasedChapters()
        {
            return await dbSet.OrderByDescending(m => m.Chapters.OrderByDescending(c => c.CreatedAt).FirstOrDefault().CreatedAt).Select(d => d.Chapters.First()).ToListAsync();
        }

        public async Task<Manga?> GetByMALId(long MALId)
        {
            return await dbSet.FirstOrDefaultAsync(m => m.MangaMyAnimeListID == MALId);
        }
    }
}
