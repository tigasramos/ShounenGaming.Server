using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaVolume : BaseEntity
    {
        public virtual FileData Image { get; set; }
        public int Number { get; set; }
        public virtual List<MangaChapter> Chapters { get; set; }
        public virtual Manga Manga { get; set; }
    }
}
