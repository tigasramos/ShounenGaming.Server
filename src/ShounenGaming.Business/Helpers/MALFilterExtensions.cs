using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Helpers
{
    public static class MALFilterExtensions
    {
        public static List<JikanDotNet.Manga> FilterCorrectTypes(this ICollection<JikanDotNet.Manga> list)
        {
            return list.Where(m => m.Type.ToLowerInvariant() != "Light Novel".ToLowerInvariant() &&
                                   m.Type.ToLowerInvariant() != "Novel".ToLowerInvariant() &&
                                   m.Type.ToLowerInvariant() != "One-shot".ToLowerInvariant()).ToList();
        }
    }
}
