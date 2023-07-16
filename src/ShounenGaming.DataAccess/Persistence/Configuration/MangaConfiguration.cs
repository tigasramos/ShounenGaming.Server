using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Persistence.Configuration
{
    public class MangaConfiguration : IEntityTypeConfiguration<Manga>
    {
        public void Configure(EntityTypeBuilder<Manga> builder)
        {
            builder.HasIndex(x => x.MangaMyAnimeListID).IsUnique();
            builder.HasIndex(x => x.MangaAniListID).IsUnique();
        }
    }
}
