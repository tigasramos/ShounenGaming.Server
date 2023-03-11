using HtmlAgilityPack;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class NeoXScansScrapper : IBaseMangaScrapper
    {
        //NeoXScans PT
        //5 seg -> 321
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            int currentPage = 1, lastPage = 1;
            do
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/page/{currentPage}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-listing-item']/div/div/div/div[@class='item-summary']");
               
                foreach(var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("div/h3/a")?.InnerText ?? "";
                    if (mangaName.Contains("[Novel]"))
                        continue;
                    var mangaUrl = manga.SelectSingleNode("div/h3/a")?.GetAttributeValue("href","") ?? "";
                    mangasList.Add(new ScrappedSimpleManga
                    {
                        Name = mangaName,
                        Link = mangaUrl.Remove(mangaUrl.Length - 1).Split("/").Last()
                    });
                }
                currentPage++;

                if (lastPage == 1)
                    lastPage = Convert.ToInt16(htmlDoc.DocumentNode.SelectNodes("//div[@class='wp-pagenavi']/a[@class='larger page']").Select(c => c.InnerText).Where(d => !string.IsNullOrEmpty(d)).Last());

            } while (currentPage <= lastPage);

            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='post-title']/h1")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='manga-excerpt']")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//ul[@class='main version-chap no-volumn']/li");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("a").InnerText.Replace("Cap.", "").Trim() ?? "";
                var chapterUrl = chapter.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapter-release-date']/i")?.InnerText ?? "";
                var parsed = DateTime.TryParseExact(chapterPageDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl.Replace("https://neoxscans.net/manga/", "").Trim(),
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
            var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='page-break no-gaps']/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("src", "").Trim());
            }
            return imagesUrls;
        }
    }
}
