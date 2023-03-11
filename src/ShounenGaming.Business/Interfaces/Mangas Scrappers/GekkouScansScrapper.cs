using HtmlAgilityPack;
using Newtonsoft.Json;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class GekkouScansScrapper : IBaseMangaScrapper
    {
        //GekkouScans PT
        //3 seg -> 112
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            var currentPage = 1;
            while (true)
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://gekkou.com.br/filterList?page={currentPage}&cat=&alpha=&sortBy=name&asc=true&author=&artist=&tag=");
                if (htmlDoc.DocumentNode.InnerText.Contains("There is no Manga!"))
                    break;

                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='media']");

                foreach(var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("div[@class='media-body']/h5/a/strong")?.InnerText ?? "";
                    var mangaUrl = manga.SelectSingleNode("div[@class='media-body']/h5/a")?.GetAttributeValue("href","") ?? "";
                    mangasList.Add(new ScrappedSimpleManga
                    {   
                        Name = mangaName,
                        Link = mangaUrl.Split("/").Last(),
                    });
                }
                currentPage++;
            }
            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://gekkou.com.br/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h2[@class='widget-title']")?.InnerText ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='well']/p")?.InnerText ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='boxed']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//h5[@class='chapter-title-rtl']/a");
            if (scrappedChapters != null)
                foreach (var chapter in scrappedChapters)
                {
                    var chapterName = chapter.InnerText.Replace(mangaName, "").Trim() ?? "";
                    var chapterUrl = chapter.GetAttributeValue("href", "") ?? "";
                    chapters.Add(new ScrappedChapter
                    {
                        Name = chapterName,
                        Link = chapterUrl.Replace("https://gekkou.com.br/manga/", "").Trim(),
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
            var htmlDoc = await web.LoadFromWebAsync($"https://gekkou.com.br/manga/{urlPart}");

            List<string> imagesUrls = new ();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@id='all']/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("data-src", ""));
            }
            return imagesUrls;
        }
    }
}
