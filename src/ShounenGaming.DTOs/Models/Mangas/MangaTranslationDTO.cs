using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaTranslationDTO
    {
        public MangaTranslationEnumDTO Language { get; set; }
        public DateTime? ReleasedDate { get; set; }

        public int ChapterId { get; set; }
        public double ChapterNumber { get; set; }

        public string MangaName { get; set; }

        public int? PreviousChapterId { get; set; }
        public int? NextChapterId { get; set; }
        public string Source { get; set; }
        public List<string> Pages { get; set; }
        public Dictionary<string, string> PageHeaders { get; set; }
    }
}
