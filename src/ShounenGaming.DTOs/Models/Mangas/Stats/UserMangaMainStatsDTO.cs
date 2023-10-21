using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas.Stats
{
    public class UserMangaMainStatsDTO
    {
        public int NumOfMangas { get; set; }
        public int NumOfChapters { get; set; }
        public double AverageScore { get; set; }
        public required Dictionary<MangaTypeEnumDTO, int> MangaTypeCounters { get; set; }
        public required Dictionary<MangaUserStatusEnumDTO, int> MangaUserStatusCounters { get; set; }
        public required Dictionary<DateOnly, int> DailyActivityCounters { get; set; }
        public required Dictionary<string, int> MangaTagsCounters { get; set; }
    }
}
