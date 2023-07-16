using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaUserData : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int MangaId { get; set; }
        public virtual Manga Manga { get; set; }

        public MangaUserStatusEnum Status { get; set; }

        public virtual List<MangaChapter> ChaptersRead { get; set; }


        public bool IsPrivate { get; set; }
    }

}
