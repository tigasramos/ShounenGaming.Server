using HtmlAgilityPack;
using Newtonsoft.Json;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.Business.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class GekkouScansScrapper : IBaseMangaScrapper
    {
        private const string BASE_URL = "https://gekkou.com.br/manga";

        //GekkouScans PT
        //3 seg -> 112
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            var currentPage = 1;
            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://gekkou.com.br/manga/page/{currentPage}/");
                    if (htmlDoc.DocumentNode.InnerText.Contains("There is no Manga!"))
                        break;

                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-12 col-md-6 badge-pos-1']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div[@class='post-title font-title']/h3/a")?.InnerText ?? "";
                        var mangaUrl = manga.SelectSingleNode("div[@class='post-title font-title']/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("div[@class='item-thumb hover-details c-image-hover']/a/img").GetAttributeValue("data-src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaUrl.Split("/").Last(),
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
            var htmlDoc = await web.LoadFromWebAsync($"{BASE_URL}/{urlPart}");
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
                        Link = chapterUrl.Replace(BASE_URL +"/", "").Trim(),
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
            var htmlDoc = await web.LoadFromWebAsync($"{BASE_URL}/{urlPart}");

            List<string> imagesUrls = new ();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@id='all']/img");
            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("data-src", ""));
            }
            return imagesUrls;
        }
        public string GetLanguage()
        {
            return "PT";
        }

        public string GetBaseURLForManga()
        {
            return BASE_URL;
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.GEKKOU_SCANS;
        }

        public async Task<List<ScrappedSimpleManga>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            var currentPage = 1;

            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://gekkou.com.br/page/{currentPage}/?s={name.Replace(" ", "+")}&post_type=wp-manga&op&author&artist&release&adult");
                    if (htmlDoc.DocumentNode.InnerText.Contains("There is no Manga!"))
                        break;

                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='row c-tabs-item__content']");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("div/div/div[@class='post-title']/h3/a")?.InnerText ?? "";
                        var mangaUrl = manga.SelectSingleNode("div/div/div[@class='post-title']/h3/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("div/div[@class='tab-thumb c-image-hover']/a/img").GetAttributeValue("data-src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaUrl.Split("/").Last(),
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
