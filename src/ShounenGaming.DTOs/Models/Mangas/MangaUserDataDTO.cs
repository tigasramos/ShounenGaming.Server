using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaUserDataDTO
    {
        public int UserId { get; set; }
        public MangaInfoDTO Manga { get; set; }

        public int FilteredReadChapters { get; set; }
        public int FilteredTotalChapters { get; set; }

        public MangaUserStatusEnumDTO Status { get; set; }
        public DateTime? AddedToStatusDate { get; set; }

        public DateTime? StartedReadingDate { get; set; }
        public DateTime? FinishedReadingDate { get; set; }
        public List<int> ChaptersRead { get; set; }
    }
}
