using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Mangas.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MangaTypeDTOEnum
    {
        MANGA, MANWHA, MANHUA
    }
}
