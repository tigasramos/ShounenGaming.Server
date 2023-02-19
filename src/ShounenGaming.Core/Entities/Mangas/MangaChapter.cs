using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaChapter : BaseEntity
    {
        public string Name { get; set; }
        public virtual List<MangaTranslation> Translations { get; set; }
    }
}
