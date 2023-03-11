using HtmlAgilityPack;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class HuntersScansScrapper : IBaseMangaScrapper
    {
        //HuntersScans PT
        //4 seg -> 78
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            var currentPage = 1;
            while (true)
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://huntersscan.xyz/series/page/{currentPage}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-item-detail manga  ']");
                if (mangasFetched == null)
                    break;

                foreach(var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("div[@class='item-summary']/div/h3/a").InnerText ?? "";
                    if (mangaName.Contains("(Novel)"))
                        continue;
                    var mangaURL = manga.SelectSingleNode("div[@class='item-summary']/div/h3/a")?.GetAttributeValue("href", "") ?? "";
                    mangasList.Add(new ScrappedSimpleManga
                    {
                        Name = mangaName,
                        Link = mangaURL.Remove(mangaURL.Length - 1).Split("/").Last(),
                    });
                }

                currentPage++;
            }

            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://huntersscan.xyz/series/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//title").InnerText.Replace("- Hunters Comics", "").Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "").Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("src", "") ?? "";


            var htmlChapters = await new HttpClient().PostAsync($"https://huntersscan.xyz/series/{urlPart}/ajax/chapters/", null);
            var htmlChaptersDoc = new HtmlDocument();
            htmlChaptersDoc.LoadHtml(await htmlChapters.Content.ReadAsStringAsync());

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlChaptersDoc.DocumentNode.SelectNodes("//ul[@class='main version-chap no-volumn']/li");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a").InnerText.Replace("Capítulo", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span/i")?.InnerText;
                var parsed = DateTime.TryParseExact(chapterPageDate, "dd/MM/yyyy",CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl.Replace("https://huntersscan.xyz/series/", ""),
                    ReleasedAt = parsed ? chapterDate : null
                });
            }

            return new ScrappedManga
            {
                Name = mangaName,
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
            };
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://huntersscan.xyz/series/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break no-gaps']/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("data-src", "").Trim());
            }
            return imagesUrls;
        }
    }
}
