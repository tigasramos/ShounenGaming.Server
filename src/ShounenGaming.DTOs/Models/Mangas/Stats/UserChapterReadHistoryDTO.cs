using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas.Stats
{
    public class UserChapterReadHistoryDTO
    {
        public DateTime ReadAt { get; set; }
        public double NumOfFirstChapter { get; set; }
        public double? NumOfLastChapter { get; set; }
        public required MangaInfoDTO Manga { get; set; }
    }
}
