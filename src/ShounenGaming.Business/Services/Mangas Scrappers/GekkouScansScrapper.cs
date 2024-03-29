﻿using HtmlAgilityPack;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Globalization;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class GekkouScansScrapper : IBaseMangaScrapper
    {
        private const string BASE_URL = "https://gekkouscans.top/manga";

        private readonly IHttpClientFactory _httpClientFactory;

        public GekkouScansScrapper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"{BASE_URL}/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='manga-title']/h1")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='well']/p")?.InnerText ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("data-src", "") ?? "";

            var httpClient = _httpClientFactory.CreateClient();
            using var htmlChapters = await httpClient.PostAsync($"{BASE_URL}{urlPart}/ajax/chapters/", null);
            var htmlChaptersDoc = new HtmlDocument();
            htmlChaptersDoc.LoadHtml(await htmlChapters.Content.ReadAsStringAsync());

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlChaptersDoc.DocumentNode.SelectNodes("//li[@class='wp-manga-chapter    ']");
            if (scrappedChapters != null)
                foreach (var chapter in scrappedChapters)
                {
                   var chapterName = chapter.SelectSingleNode("a").InnerText.Replace(mangaName, "").Replace("Capítulo", "").Trim() ?? "";
                    var chapterUrl = chapter.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                    var releasedDate = chapter.SelectSingleNode("span/i")?.InnerText;
                    DateTime convertedDate = DateTime.Now; 
                    var dateConverted = releasedDate is null ? false : DateTime.TryParseExact(releasedDate.Replace(" de ", "/"), "d/MMMM/yyyy", new CultureInfo("pt-BR"), DateTimeStyles.None, out convertedDate);
                    
                    chapters.Add(new ScrappedChapter
                    {
                        Name = chapterName.Trim(),  
                        Link = chapterUrl.Replace(BASE_URL +"/", "").Trim(),
                        ReleasedAt = dateConverted ? DateTime.SpecifyKind(convertedDate, DateTimeKind.Utc) : null,
                    });
                }

            return new ScrappedManga
            {
                Name = mangaName.Trim(),
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"{BASE_URL}/{urlPart}?style=list");

            List<string> imagesUrls = new ();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break ']/img");
            if (images == null)
                images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break no-gaps']/img"); 

            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("data-src", "").ToString().Trim());
            }
            return imagesUrls;
        }
        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public string GetBaseURLForManga()
        {
            return BASE_URL;
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.GEKKOU_SCANS;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            var currentPage = 1;

            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://gekkouscans.top/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga&op&author&artist&release&adult");
                if (htmlDoc.DocumentNode.InnerText.Contains("There is no Manga!"))
                    return mangasList; 

                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                if (mangasFetched == null || !mangasFetched.Any())
                    return mangasList; 

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("div/div/div[@class='post-title']/h3/a")?.InnerText ?? "";
                    var mangaUrl = manga.SelectSingleNode("div/div/div[@class='post-title']/h3/a")?.GetAttributeValue("href", "") ?? "";
                    var imageUrl = manga.SelectSingleNode("div/div[@class='tab-thumb c-image-hover']/a/img")?.GetAttributeValue("data-src", "") ?? "";
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = mangaName.Trim(),
                        Url = mangaUrl.Split("/").TakeLast(2).First(),
                        ImageURL = imageUrl,
                        Source = GetMangaSourceEnumDTO()
                    });
                }

            }
            catch(Exception ex)
            {
                Log.Error($"GekkouScans - SearchManga: {ex.Message}");
            }
           
            return mangasList;
        }
    }
}
