using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Persistence.Configuration
{
    public class ServerMemberConfiguration : IEntityTypeConfiguration<ServerMember>
    {
        public void Configure(EntityTypeBuilder<ServerMember> builder)
        {
            builder.HasIndex(x => x.DiscordId).IsUnique();
            builder.HasIndex(x => x.Username).IsUnique();
        }
    }
}
