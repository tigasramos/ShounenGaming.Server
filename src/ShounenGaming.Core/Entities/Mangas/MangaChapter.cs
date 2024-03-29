﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaChapter : BaseEntity
    {
        public double Name { get; set; }
        public virtual List<MangaTranslation> Translations { get; set; }


        public int MangaId { get; set; }
        public virtual Manga Manga { get; set; }
    }
}
