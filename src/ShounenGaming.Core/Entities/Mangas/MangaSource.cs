using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaSource : BaseEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string? ImageURL { get; set; }
        public string Source { get; set; }


        public int MangaId { get; set; }
    }
}
