using HtmlAgilityPack;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas.Enums;
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
            int currentPage = 1;
            

            try { 
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/manga/page/{currentPage}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-6 col-md-2 badge-pos-2']/div");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div[@class='item-summary']/div/h3/a")?.InnerText ?? "";
                        if (mangaName.Contains("[Novel]"))
                            continue;
                        var mangaUrl = manga.SelectSingleNode("div[@class='item-summary']/div/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("div[@class='item-thumb  c-image-hover']/a/img").GetAttributeValue("src", "") ?? "";
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
            } catch { }

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
                Source = GetMangaSourceEnumDTO()
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
        public string GetLanguage()
        {
            return "PT";
        }

        public string GetBaseURLForManga()
        {
            return "https://neoxscans.net/manga";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.NEO_X_SCANS;
        }

        public async Task<List<ScrappedSimpleManga>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            int currentPage = 1;

            try { 
                while(true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://neoxscans.net/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a")?.InnerText ?? "";
                        if (mangaName.Contains("[Novel]"))
                            continue;
                        var mangaUrl = manga.SelectSingleNode("div/div[@class='tab-summary']/div/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("div/div[@class='tab-thumb c-image-hover']/a/img").GetAttributeValue("src", "") ?? "";
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
            } catch { }

            return mangasList;
        }
    }
}
