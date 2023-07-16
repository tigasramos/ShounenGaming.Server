using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaAlternativeName : SimpleEntity
    {
        public string Language { get; set; }
        public string Name { get; set; }

        public int MangaId { get; set; } 
    }
}
