using ShounenGaming.Core.Entities.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class UserMangasConfigurations : SimpleEntity
    {
        public ReadingModeTypeEnum ReadingMode { get; set; } = ReadingModeTypeEnum.HORIZONTAL_MANGAS_OTHERS_VERTICAL;
        public NSFWBehaviourEnum NSFWBehaviour { get; set; } = NSFWBehaviourEnum.NONE;
        public TranslationLanguageEnum TranslationLanguage { get; set; } = TranslationLanguageEnum.PT;
        public bool SkipChapterToAnotherTranslation { get; set; } = true;
        public bool ShowProgressForChaptersWithDecimals { get; set; } = true;

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
