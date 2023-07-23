using System.Text.Json.Serialization;

namespace ShounenGaming.DTOs.Models.Mangas.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MangaSourceEnumDTO
    {
        GEKKOU_SCANS = 0,
        NEO_X_SCANS = 1,
        MANGANATO = 2,
        SILENCE_SCANS = 3,
        HUNTERS_SCANS = 4,
        BR_MANGAS = 5,
        MANGAS_CHAN = 6,
        DISKUS_SCAN = 7,
        UNION_MANGAS = 8,
        YES_MANGAS = 9,
        MANGAS_DEX_PT = 10,
        MANGAS_DEX_EN = 11
    }

}
