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
        public virtual FileData Image { get; set; }
        public bool IsReleasing { get; set; }
        public virtual MangaWriter Writer { get; set; }
        public MangaType Type { get; set; }
        public DateTime? LastChapterReleaseAt { get; set; }
        public DateTime? FirstChapterReleaseAt { get; set; }
        public virtual List<MangaTag> Tags { get; set; }

        public virtual List<MangaChapter> Chapters { get; set; }

        public int? MangaMyAnimeListID { get; set; }
        public int? MangaAniListID { get; set; }
    }
}
