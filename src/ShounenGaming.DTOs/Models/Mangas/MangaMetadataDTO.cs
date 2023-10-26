using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaMetadataDTO
    {
        public long Id { get; set; }
        public MangaMetadataSourceEnumDTO  Source { get; set; }

        public bool AlreadyExists { get; set; }

        public List<string> Titles { get; set; }
        public string ImageUrl { get; set; }
        public MangaTypeEnumDTO Type { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal? Score { get; set; }
        public List<string> Tags { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
