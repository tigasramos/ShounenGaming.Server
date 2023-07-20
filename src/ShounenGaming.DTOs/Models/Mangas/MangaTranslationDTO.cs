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
        public int ChapterId { get; set; }
        public double ChapterNumber { get; set; }
        public MangaTranslationEnumDTO Language { get; set; }
        public required string Source { get; set; }
        public List<string> Pages { get; set; } = new List<string>();
        public Dictionary<string, string>? PageHeaders { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public required string MangaName { get; set; }

        public int? PreviousChapterId { get; set; }
        public int? NextChapterId { get; set; }
    }
}
