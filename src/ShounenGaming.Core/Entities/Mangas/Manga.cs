﻿using ShounenGaming.Core.Entities.Mangas.Enums;

namespace ShounenGaming.Core.Entities.Mangas
{
    public class Manga : BaseEntity
    {
        public string Name { get; set; }
        public virtual List<MangaAlternativeName> AlternativeNames { get; set; }
        public virtual List<MangaSynonym> Synonyms { get; set; }
        public string Description { get; set; }
        public bool IsReleasing { get; set; }
        public List<string> ImagesUrls { get; set; }
        public virtual MangaWriter Writer { get; set; }
        public MangaTypeEnum Type { get; set; }
        public virtual List<MangaTag> Tags { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }


        public virtual List<MangaChapter> Chapters { get; set; }
        public virtual List<MangaUserData> UsersData { get; set; }

        //Configurations
        public bool IsNSFW { get; set; } = false;
        public bool IsSeasonManga { get; set; }

        public long? MangaMyAnimeListID { get; set; }
        public int? MALPopularity { get; set; }
        public double? MALScore { get; set; }

        public long? MangaAniListID { get; set; }
        public int? ALPopularity { get; set; }
        public double? ALScore { get; set; }

        public virtual List<MangaSource> Sources { get; set; }

    }
        
}
