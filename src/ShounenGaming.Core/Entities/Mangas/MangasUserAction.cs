using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangasUserAction : BaseEntity
    {
        public MangaUserActionEnum Action { get; set; }
        public virtual Manga Manga { get; set; }
    }
    public enum MangaUserActionEnum
    {
        CHANGED_STATUS,
        CHAPTER_READ,
    }
}
