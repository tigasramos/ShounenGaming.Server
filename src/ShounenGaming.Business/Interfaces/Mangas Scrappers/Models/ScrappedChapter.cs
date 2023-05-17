using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models
{
    public class ScrappedChapter
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }
}
