using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas.Enums;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangasUserActivityDTO
    {
        public required SimpleUserDTO User { get; set; }
        public required MangaInfoDTO Manga { get; set; }
        public UserActivityTypeEnumDTO ActivityType { get; set; }
        public DateTime MadeAt { get; set; }

        // Chapter Related
        public double? FirstChapterRead { get; set; }
        public double? LastChapterRead { get; set; }

        //Status Related
        public MangaUserStatusEnumDTO? PreviousState { get; set; }
        public MangaUserStatusEnumDTO? NewState { get; set; }
    }
    
}
