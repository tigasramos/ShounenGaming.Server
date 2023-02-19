using ShounenGaming.Core.Entities.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaTranslation : BaseEntity
    {
        public TranslationLanguage Language { get; set; }
        public virtual List<MangaChapterPage> Pages { get; set; }
        public DateTime? ReleasedDate { get; set; }
    }
}
