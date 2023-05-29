using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaSource : BaseEntity
    {
        public string Provider { get; set; }
        public string URL { get; set; }
        public bool BrokenLink { get; set; }


        public int MangaId { get; set; }
    }
}
