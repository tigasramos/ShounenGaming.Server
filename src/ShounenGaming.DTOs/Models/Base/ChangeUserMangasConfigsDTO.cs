using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Base
{
    public class ChangeUserMangasConfigsDTO
    {
        public ReadingModeTypeEnumDTO? ReadingMode { get; set; }
        public NSFWBehaviourEnumDTO? NSFWBehaviour { get; set; }
        public MangaTranslationEnumDTO? TranslationLanguage { get; set; }
        public bool? SkipChapterToAnotherTranslation { get; set; }
        public bool? ShowProgressForChaptersWithDecimals { get; set; }
    }
}
