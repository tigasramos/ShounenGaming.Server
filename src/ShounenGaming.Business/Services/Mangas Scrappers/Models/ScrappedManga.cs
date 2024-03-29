﻿using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Services.Mangas_Scrappers.Models
{
    public class ScrappedManga
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public List<ScrappedChapter> Chapters { get; set; }
        public MangaSourceEnumDTO Source { get; set; }
    }
}
