using System.Text.Json.Serialization;

namespace ShounenGaming.DTOs.Models.Mangas.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MangaTypeEnumDTO
    {
        MANGA, MANWHA, MANHUA
    }
}
