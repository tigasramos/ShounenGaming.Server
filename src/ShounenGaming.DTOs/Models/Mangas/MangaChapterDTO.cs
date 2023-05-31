using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaChapterDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<MangaTranslationInfoDTO> Translations { get; set; }
    }
}
