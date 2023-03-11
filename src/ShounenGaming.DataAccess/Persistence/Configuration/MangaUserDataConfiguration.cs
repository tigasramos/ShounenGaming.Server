using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Persistence.Configuration
{
    public class MangaUserDataConfiguration : IEntityTypeConfiguration<MangaUserData>
    {
        public void Configure(EntityTypeBuilder<MangaUserData> builder)
        {
            builder.HasMany(b => b.ChaptersRead).WithMany();
        }
    }
}
