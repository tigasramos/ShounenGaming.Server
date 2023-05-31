﻿using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaTranslationDTO
    {
        public int Id { get; set; }
        public MangaTranslationEnumDTO Language { get; set; }
        public DateTime? ReleasedDate { get; set; }

        public int ChapterId { get; set; }
        public string ChapterNumber { get; set; }

        public string MangaName { get; set; }

        public int? PreviousChapterTranslationId { get; set; }
        public int? NextChapterTranslationId { get; set; }
        public List<string> Pages { get; set; }
    }
}
