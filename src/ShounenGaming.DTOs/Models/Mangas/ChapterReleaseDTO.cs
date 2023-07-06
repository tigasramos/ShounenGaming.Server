
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.DTOs.Models.Mangas
{

    public class ChapterReleaseDTO
    {
        public int Id { get; set; }
        public double Name { get; set; }
        public MangaTranslationEnumDTO Translation { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
