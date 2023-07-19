using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Helpers
{
    public static class MangasHelper
    {
        public static bool IsMangaNSFW(IEnumerable<string> tags)
        {
            // Add More Tags if Needed
            var nsfwTags = new List<string>() 
            { 
                "erotica", "hentai"
            };

            return tags.Any(t => nsfwTags.Contains(t.ToLowerInvariant()));
        }

        public static List<JikanDotNet.Manga> FilterCorrectTypes(this ICollection<JikanDotNet.Manga> list)
        {
            return list.Where(m => m.Type.ToLowerInvariant() != "Light Novel".ToLowerInvariant() &&
                                   m.Type.ToLowerInvariant() != "Novel".ToLowerInvariant() &&
                                   m.Type.ToLowerInvariant() != "One-shot".ToLowerInvariant()).ToList();
        }
    }
}
