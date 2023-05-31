﻿using HtmlAgilityPack;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class ManganatoScrapper : IBaseMangaScrapper
    {
        //Manganato EN
        //6:42 seg -> 35133
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            int currentPage = 1;

            try { 
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://manganato.com/genre-all/{currentPage}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='content-genres-item']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div/h3/a")?.InnerText ?? "";
                        var mangaURL = manga.SelectSingleNode("div/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("a/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaURL.Split("-").Last(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;
                } 
            }
            catch { }

            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://chapmanganato.com/manga-{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='story-info-right']/h1")?.InnerText ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='panel-story-info-description']")?.InnerText.Replace("Description :", "").Replace("\n","").Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='info-image']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//li[@class='a-h']");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a")?.InnerText.Replace("Chapter", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapter-time text-nowrap']")?.GetAttributeValue("title", "");
                var parsed = DateTime.TryParseExact(chapterPageDate, "MMM dd,yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl.Replace("https://chapmanganato.com/manga-", "").Trim(),
                    ReleasedAt = parsed ? chapterDate.AddHours(-8) : null
                });
            }

            return new ScrappedManga
            {
                Name = mangaName,
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }
        //NOTE: Needs Header Referer:https://chapmanganato.com/ to work
        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://chapmanganato.com/manga-{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='container-chapter-reader']/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("src", "").Trim());
            }
            return imagesUrls;
        }
        public string GetLanguage()
        {
            return "EN";
        }

        public string GetBaseURLForManga()
        {
            return "https://chapmanganato.com";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.MANGANATO;
        }

        public async Task<List<ScrappedSimpleManga>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            int currentPage = 1;

            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://manganato.com/search/story/{name.Replace(" ", "_")}?page={currentPage}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='search-story-item']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div/h3/a")?.InnerText ?? "";
                        var mangaURL = manga.SelectSingleNode("div/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("a/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaURL.Split("-").Last(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;

                }
            } catch { }

            return mangasList;
        }
    }
}