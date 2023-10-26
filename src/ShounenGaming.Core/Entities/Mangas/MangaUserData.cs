using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas.Enums;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class MangaUserData : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int MangaId { get; set; }
        public virtual Manga Manga { get; set; }

        public MangaUserStatusEnum Status { get; set; }
        public double? Rating { get; set; }

        public virtual List<MangaChapter> ChaptersRead { get; set; }
    }

}
