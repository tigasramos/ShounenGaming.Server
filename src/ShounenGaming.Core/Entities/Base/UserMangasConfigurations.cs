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
        public ReadingModeTypeEnum ReadingMode { get; set; } = ReadingModeTypeEnum.ALWAYS_VERTICAL;
        public NSFWBehaviourEnum NSFWBehaviour { get; set; } = NSFWBehaviourEnum.NONE;
        public TranslationLanguageEnum TranslationLanguage { get; set; } = TranslationLanguageEnum.PT;

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
