using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Mangas
{
    public class MangaSourceDTO
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string? ImageURL { get; set; }
        public MangaSourceEnumDTO Source { get; set; }
    }
}
