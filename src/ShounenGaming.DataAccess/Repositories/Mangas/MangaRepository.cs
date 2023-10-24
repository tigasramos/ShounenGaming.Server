using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{
    public class MangaRepository : BaseRepository<Manga>, IMangaRepository
    {
        public MangaRepository(DbContext context) : base(context)
        {
        }
        
        public async Task<List<Manga>> GetSeasonMangas()
        {
            return await dbSet.Where(m => m.IsSeasonManga).OrderByDescending(m => ((m.MALPopularity ?? m.ALPopularity) + (m.ALPopularity ?? m.MALPopularity)) / 2).ToListAsync();
        }
        public async Task<List<Manga>> GetWaitingMangas()
        {
            return await dbSet.Where(m => m.UsersData.Any(ud => ud.Status == MangaUserStatusEnum.PLANNED || ud.Status == MangaUserStatusEnum.READING || ud.Status == MangaUserStatusEnum.ON_HOLD) && !m.Sources.Any()).OrderBy(m => m.UsersData.OrderBy(us => us.UpdatedAt).First().UpdatedAt).ToListAsync();
        }
        public async Task<List<Manga>> GetPopularMangas(bool includeNSFW = true)
        {
            var query = dbSet.AsQueryable();

            if (!includeNSFW)
            {
                query = query.Where(m => !m.IsNSFW);
            }

            return await query.OrderByDescending(m => ((m.MALPopularity ?? m.ALPopularity) + (m.ALPopularity ?? m.MALPopularity)) / 2).Take(10).ToListAsync();
        }

        public override void DeleteDependencies(Manga entity)
        {
            // Delete UserData
            context.RemoveRange(entity.UsersData);

            // Delete Sources
            context.Set<MangaSource>().RemoveRange(entity.Sources);

            // Delete Synonyms Names
            context.RemoveRange(entity.Synonyms);

            // Delete Alternative Names
            context.RemoveRange(entity.AlternativeNames);

            // Delete Writer
            if (entity.Writer.Mangas.Count == 1)
            {
                context.Remove(entity.Writer);
            }

            // Delete Chapters
            foreach (var chapter in entity.Chapters)
            {
                context.RemoveRange(chapter.Translations);
            }
            context.RemoveRange(entity.Chapters);
        }

        public async Task<List<Manga>> SearchManga(int page, bool includeNSFW = true, string? name = null, int? userId = null)
        {
            return await SearchQuery(includeNSFW, name, userId).OrderBy(c => c.Name).Skip(25 * (page - 1)).Take(25).ToListAsync();
        }

        public async Task<int> GetAllCount(bool includeNSFW = true, string? name = null, int? userId = null)
        {
            return await SearchQuery(includeNSFW, name, userId).CountAsync();
        }

        private IQueryable<Manga> SearchQuery(bool includeNSFW = true, string? name = null, int? userId = null)
        {
            IQueryable<Manga> query = dbSet;

            if (userId is not null)
                query = query.Where(m => !m.UsersData.Any(ud => ud.UserId == userId && ud.Status == MangaUserStatusEnum.IGNORED));

            if (!includeNSFW)
                query = query.Where(m => !m.IsNSFW);

            name = name?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(name))
                query = query.Where(m => m.Name.ToLower().Contains(name) || m.AlternativeNames.Any(d => d.Name.ToLower().Contains(name)) || m.Synonyms.Any(s => s.Name.ToLower().Contains(name)));
          

            return query;
        }

        public async Task<List<Manga>> GetRecentlyAddedMangas()
        {
            return await dbSet.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }

        public async Task<List<Manga>> GetRecentlyReleasedChapters(bool includeNSFW = true)
        {
            var query =  dbSet
                .Where(m => m.Chapters.Any());

            if (!includeNSFW)
                query = query.Where(c => !c.IsNSFW);

            var chapters = await query
                .OrderByDescending(c => c.Chapters.OrderByDescending(c => c.Name).First().CreatedAt)
                .Take(10)
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

        public async Task<List<Manga>> GetMangasByTag(string tag, bool includeNSFW = true)
        {
            var query = dbSet
                .Where(m => m.Tags.Any(t => t.Name == tag));

            if (!includeNSFW)
                query = query.Where(c => !c.IsNSFW);

            var mangas = await query.ToListAsync();
            return mangas;
        }
    }
}
