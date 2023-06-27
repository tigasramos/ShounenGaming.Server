
namespace ShounenGaming.DTOs.Models.Mangas
{
    public class ChapterReleaseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public MangaInfoDTO Manga { get; set; }
        public List<MangaTranslationInfoDTO> Translations { get; set; }
    }
}
