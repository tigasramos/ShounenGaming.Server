using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsReleasing { get; set; }
        public MangaTypeEnumDTO Type { get; set; }
        public List<string> Tags { get; set; }
        public int ChaptersCount { get; set; }
        public string ImageUrl { get; set; }
        public int? MyAnimeListId { get; set; }
        public int? AnilistId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime? LastChapterDate { get; set; }
    }
}
