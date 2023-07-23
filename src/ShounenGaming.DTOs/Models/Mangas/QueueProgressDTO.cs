using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class QueueProgressDTO
    {
        public double Percentage { get; set; }
        public MangaSourceEnumDTO CurrentSource { get; set; }
        public int CurrentChapter { get; set; }
        public int TotalChapters { get; set; }
    }
}
