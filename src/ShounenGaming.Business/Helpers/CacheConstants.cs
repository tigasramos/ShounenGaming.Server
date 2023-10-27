using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Helpers
{
    internal static class CacheConstants
    {
        public static readonly string recent_mangas = "recent_mangas";
        public static readonly string added_mangas_action = "all_added_mangas";
        public static readonly string status_changed_action = "all_status_changed";
        public static readonly string chapters_changed_action = "all_chapters_changed";
        public static string manga_dto_byId(int id) => $"manga_${id}_dto";
        public static string manga_sources_dto_byId(int id) => $"manga_sources_{id}_dto";
        public static string manga_sources_byName(string treatedName) => $"manga_sources_{treatedName}";
        public static readonly string season_mangas = "season_mangas";

        public static readonly string manga_tags = "manga_tags";
        public static string manga_tags_includesNSFW(string tag, bool includesNSFW) => $"manga_tag_{tag.ToLower()}_{includesNSFW}";

        public static string recent_chapters_includesNSFW(bool includeNSFW) => $"recent_chapters_{includeNSFW}";


    }
}
