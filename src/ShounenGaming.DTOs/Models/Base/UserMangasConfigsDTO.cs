using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.DTOs.Models.Base
{
    public class UserMangasConfigsDTO
    {
        public ReadingModeTypeEnumDTO ReadingMode { get; set; }
        public NSFWBehaviourEnumDTO NSFWBehaviour { get; set; }
        public MangaTranslationEnumDTO  TranslationLanguage { get; set; }
    }
}
