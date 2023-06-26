using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Mangas
{
    public abstract class MangasUserAction : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class ChangedMangaStatusAction : MangasUserAction 
    {
        public MangaUserStatusEnum? PreviousState { get; set; }
        public MangaUserStatusEnum? NewState { get; set; }
        public int MangaId { get; set; }
        public virtual Manga Manga { get; set; }
    }

    public class ChangedChapterStateAction : MangasUserAction
    {
        public int ChapterId { get; set; }
        public virtual MangaChapter Chapter { get; set; }
        public bool Read { get; set; }
    }

    public class AddedMangaAction : MangasUserAction
    {
        public int MangaId { get; set; }
        public virtual Manga Manga { get; set; }
    }
}
