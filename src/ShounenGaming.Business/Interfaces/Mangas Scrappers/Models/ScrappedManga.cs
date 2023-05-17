using ShounenGaming.Business.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models
{
    public class ScrappedManga
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public List<ScrappedChapter> Chapters { get; set; }
        public MangaSourceEnumDTO Source { get; set; }
    }
}
