using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class Manga : BaseEntity
    {
        public string Name { get; set; }
        public virtual List<MangaAlternativeName> AlternativeNames { get; set; }
        public string Description { get; set; }
        public bool IsReleasing { get; set; }
        public virtual MangaWriter Writer { get; set; }
        public MangaType Type { get; set; }
        public virtual List<MangaTag> Tags { get; set; }

        public virtual List<MangaChapter> Chapters { get; set; }

        public long? MangaMyAnimeListID { get; set; }
        public long? MangaAniListID { get; set; }

        public virtual List<MangaSource> Sources { get; set; }

        public virtual List<MangaUserData> UsersData { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
        
}
