﻿using ShounenGaming.Business.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Mangas
{
    public class MangaDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Dictionary<string, string> AlternativeNames { get; set; }
        public string Description { get; set; }
        public bool IsReleasing { get; set; }
        public MangaWriterDTO Writer { get; set; }
        public MangaTypeDTOEnum Type { get; set; }
        public List<string> Tags { get; set; }

        public List<MangaChapterDTO> Chapters { get; set; }

        public string ImageUrl { get; set; }

        public long? MangaMyAnimeListID { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

    }
}
