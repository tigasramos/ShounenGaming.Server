using HtmlAgilityPack;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class MangasChanScrapper : IBaseMangaScrapper
    {
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            int currentPage = 1;
           
            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://mangaschan.com/manga/?page={currentPage}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listupd']/div/div");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("a/div[@class='bigor']/div[@class='tt']")?.InnerText.Replace("\n", "") ?? "";
                        var mangaUrl = manga.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("a/div[@class='limit']/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaUrl.Remove(mangaUrl.Length - 1).Split("/").Last(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;

                }

            } catch {  }

            return mangasList;
        }

        public string GetBaseURLForManga()
        {
            return "https://mangaschan.com/";
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://mangaschan.com/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@id='readerarea']/noscript/p/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("src", "").Trim());
            }
            return imagesUrls;
        }

        public string GetLanguage()
        {
            return "PT";
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://mangaschan.com/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='entry-title']")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='entry-content entry-content-single']/p")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='thumb']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//ul[@class='clstyle']/li/div/div/a");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("span[@class='chapternum']").InnerText.Replace("Capítulo", "").Trim() ?? "";
                var chapterUrl = chapter.GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapterdate']")?.InnerText ?? "";
                var parsed = DateTime.TryParseExact(chapterPageDate, "MMMM d, yyyy", CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl.Replace("https://mangaschan.com/", "").Trim(),
                    ReleasedAt = parsed ? chapterDate : null
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

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.MANGAS_CHAN;
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
                    var htmlDoc = await web.LoadFromWebAsync($"https://mangaschan.com/page/{currentPage}/?s={name.Replace(" ", "+")}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listupd']/div/div");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("a/div[@class='bigor']/div[@class='tt']")?.InnerText.Replace("\n", "") ?? "";
                        var mangaUrl = manga.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("a/div[@class='limit']/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaUrl.Remove(mangaUrl.Length - 1).Split("/").Last(),
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
    }
}
