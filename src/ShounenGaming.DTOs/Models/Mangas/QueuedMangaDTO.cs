using ShounenGaming.DTOs.Models.Base;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class QueuedMangaDTO
    {
        public MangaInfoDTO Manga { get; set; }
        public QueueProgressDTO? Progress { get; set; }
        public SimpleUserDTO? QueuedByUser { get; set; }
        public DateTime QueuedAt { get; set; }
    }
}
