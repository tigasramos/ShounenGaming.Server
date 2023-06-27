using ShounenGaming.DTOs.Models.Mangas.Enums;
namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaTranslationInfoDTO
    {
        public int Id { get; set; } // TODO: Maybe Remove
        public MangaTranslationEnumDTO Language { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
