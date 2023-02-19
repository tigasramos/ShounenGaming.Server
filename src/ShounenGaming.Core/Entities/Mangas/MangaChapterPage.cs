using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaChapterPage : BaseEntity
    {
        public int Order { get; set; }
        public virtual FileData Image { get; set; }
    }
}
