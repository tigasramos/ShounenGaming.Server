using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
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
        
        public async Task<List<Manga>> GetWaitingMangas()
        {
            return await dbSet.Where(m => m.UsersData.Any(ud => ud.Status == MangaUserStatusEnum.PLANNED) && !m.Sources.Any()).OrderBy(m => m.UsersData.OrderBy(us => us.UpdatedAt).First().UpdatedAt).ToListAsync();
        }
        public async Task<List<Manga>> GetPopularMangas()
        {
            return await dbSet.OrderByDescending(m => ((m.MALPopularity ?? m.ALPopularity) + (m.ALPopularity ?? m.MALPopularity)) / 2).Take(10).ToListAsync();
        }
        public async Task<List<Manga>> GetFeaturedMangas()
        {
            return await dbSet.Where(m => m.IsFeatured).ToListAsync();
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

        public async Task<List<Manga>> SearchManga(int page, string? name = null, int? userId = null)
        {
            return await SearchQuery(name, userId).OrderBy(c => c.Name).Skip(25 * (page - 1)).Take(25).ToListAsync();
        }

        public async Task<int> GetAllCount(string? name = null, int? userId = null)
        {
            return await SearchQuery(name, userId).CountAsync();
        }

        private IQueryable<Manga> SearchQuery(string? name = null, int? userId = null)
        {
            IQueryable<Manga> query = dbSet;

            if (userId is not null)
                query = query.Where(m => !m.UsersData.Any(ud => ud.UserId == userId && ud.Status == MangaUserStatusEnum.IGNORED));


            if (!string.IsNullOrEmpty(name))
                query = query.Where(m => m.Name.ToLower().Contains(name) || m.AlternativeNames.Any(d => d.Name.ToLower().Contains(name)) || m.Synonyms.Any(s => s.Name.ToLower().Contains(name)));


            return query;
        }

        public async Task<List<Manga>> GetRecentlyAddedMangas()
        {
            return await dbSet.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }

        public async Task<List<Manga>> GetRecentlyReleasedChapters()
        {
            var chapters = await dbSet
                .Where(m => m.Chapters.Any())
                .OrderByDescending(c => c.Chapters.OrderByDescending(c => c.Name).First().CreatedAt)
                .ToListAsync();
            return chapters;
        }

        public async Task<Manga?> GetByMALId(long MALId)
        {
            return await dbSet.FirstOrDefaultAsync(m => m.MangaMyAnimeListID == MALId);
        }

        public async Task<Manga?> GetByChapter(int chapterId)
        {
            return await dbSet.FirstOrDefaultAsync(m => m.Chapters.Any(c => c.Id == chapterId));
        }

        public async Task<Manga?> GetByChapters(List<int> chaptersIds)
        {
            return await dbSet.SingleOrDefaultAsync(m => m.Chapters.Any(c => chaptersIds.Contains(c.Id)));
        }


        public async Task<Manga?> GetByALId(long alId)
        {
            return await dbSet.Where(m => m.MangaAniListID == alId).FirstOrDefaultAsync();
        }

        public async Task<Manga?> ClearSources(int mangaId)
        {
            var manga = await dbSet.FirstOrDefaultAsync(m => m.Id == mangaId);
            context.Set<MangaSource>().RemoveRange(manga.Sources);
            await context.SaveChangesAsync();

            return await dbSet.FirstOrDefaultAsync(m => m.Id == mangaId);
        }

        public async Task<Manga?> ClearTranslations(int mangaId, IEnumerable<MangaTranslation> translations)
        {
            context.Set<MangaTranslation>().RemoveRange(translations);
            await context.SaveChangesAsync();

            return await dbSet.FirstOrDefaultAsync(m => m.Id == mangaId);
        }

        public async Task<bool> MangaExistsByMAL(long malId)
        {
            return await dbSet.AnyAsync(m => m.MangaMyAnimeListID == malId);
        }

        public async Task<bool> MangaExistsByAL(long alId)
        {
            return await dbSet.AnyAsync(m => m.MangaAniListID == alId);
        }
    }
}
