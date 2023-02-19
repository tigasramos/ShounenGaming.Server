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
        public DbSet<Bot> Bots { get; set; }
        public DbSet<FileData> Files { get; set; }

        //Tierlist
        public DbSet<Tierlist> Tierlists { get; set; }
        public DbSet<Tier> Tiers { get; set; }
        public DbSet<TierlistItem> TierlistItems { get; set; }
        public DbSet<TierlistCategory> TierlistCategories { get; set; }
        public DbSet<TierChoice> TierChoices { get; set; }
        public DbSet<UserTierlist> UserTierlists { get; set; }


        public DbSet<Manga> Mangas { get; set; }
        public DbSet<MangaWriter> MangaWriters { get; set; }


        public ShounenGamingContext(DbContextOptions<ShounenGamingContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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
