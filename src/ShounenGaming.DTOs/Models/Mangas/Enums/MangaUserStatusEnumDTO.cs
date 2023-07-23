using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MangaUserStatusEnumDTO
    {
        READING = 0,
        PLANNED = 1,
        DROPPED = 2,
        IGNORED = 3,
        COMPLETED = 4,
        ON_HOLD = 5
    }
}
