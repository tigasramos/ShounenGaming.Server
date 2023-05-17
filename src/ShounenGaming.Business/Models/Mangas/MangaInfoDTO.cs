using ShounenGaming.Business.Models.Mangas.Enums;

namespace ShounenGaming.Business.Models.Mangas
{
    public class MangaInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsReleasing { get; set; }
        public MangaTypeDTOEnum Type { get; set; }
        public List<string> Tags { get; set; }
        public int ChaptersCount { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
