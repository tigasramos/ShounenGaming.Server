using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaTranslationInfoDTO
    {
        public int Id { get; set; }
        public MangaTranslationEnumDTO Language { get; set; }
        public DateTime? ReleasedDate { get; set; }
    }
}
