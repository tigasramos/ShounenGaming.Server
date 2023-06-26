using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Persistence
{
    public class ShounenGamingContext : DbContext
    {
        //Base
        public DbSet<User> Users { get; set; }
        public DbSet<ServerMember> ServerMembers { get; set; }

        //Tierlist
        //public DbSet<Tierlist> Tierlists { get; set; }
        //public DbSet<Tier> Tiers { get; set; }
        //public DbSet<TierlistItem> TierlistItems { get; set; }
        //public DbSet<TierlistCategory> TierlistCategories { get; set; }
        //public DbSet<TierChoice> TierChoices { get; set; }
        //public DbSet<UserTierlist> UserTierlists { get; set; }

        //Mangas
        public DbSet<Manga> Mangas { get; set; }
        public DbSet<MangaAlternativeName> MangaAlternativeNames { get; set; }
        public DbSet<MangaChapter> MangaChapters { get; set; }
        public DbSet<MangaSource> MangaSources { get; set; }
        public DbSet<MangaWriter> MangaWriters { get; set; }
        public DbSet<MangaTag> MangaTags { get; set; }
        public DbSet<MangaTranslation> MangaTranslations { get; set; }

        public DbSet<MangaUserData> MangaUsersData { get; set; }
        public DbSet<ChangedChapterStateAction> MangaChaptersHistory { get; set; }
        public DbSet<ChangedMangaStatusAction> MangaStatusHistory { get; set; }
        public DbSet<AddedMangaAction> AddedMangaHistory { get; set; }


        public ShounenGamingContext(DbContextOptions<ShounenGamingContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(ShounenGamingContext)));

            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
