using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public async Task<List<MangaSourceDTO>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
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
                        mangasList.Add(new MangaSourceDTO
                        {
                            Name = mangaName.Trim(),
                            Url = mangaUrl.Split("/").Last(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;

                }
            }
            catch(Exception ex)
            {
                Log.Error($"GekkouScans - GetAllMangas: {ex.Message}");
            }
            
            return mangasList;
        }

        public async Task<List<MangaSourceDTO>> GetAllMangasByPage(int page)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://gekkou.com.br/manga/page/{page}/");
                if (htmlDoc.DocumentNode.InnerText.Contains("There is no Manga!"))
                    return new List<MangaSourceDTO>();

                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-12 col-md-6 badge-pos-1']");
                if (mangasFetched == null || !mangasFetched.Any()) return new List<MangaSourceDTO>();

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("div[@class='post-title font-title']/h3/a")?.InnerText ?? "";
                    var mangaUrl = manga.SelectSingleNode("div[@class='post-title font-title']/h3/a")?.GetAttributeValue("href", "") ?? "";
                    var imageUrl = manga.SelectSingleNode("div[@class='item-thumb hover-details c-image-hover']/a/img").GetAttributeValue("data-src", "") ?? "";
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = mangaName.Trim(),
                        Url = mangaUrl.Split("/").Last(),
                        ImageURL = imageUrl,
                        Source = GetMangaSourceEnumDTO()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GekkouScans - GetAllMangasByPage: {ex.Message}");
            }

            return mangasList;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"{BASE_URL}/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='manga-title']/h1")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='well']/p")?.InnerText ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='summary_image']/a/img")?.GetAttributeValue("data-src", "") ?? "";


            var htmlChapters = await new HttpClient().PostAsync($"https://gekkou.com.br/manga/{urlPart}/ajax/chapters/", null);
            var htmlChaptersDoc = new HtmlDocument();
            htmlChaptersDoc.LoadHtml(await htmlChapters.Content.ReadAsStringAsync());

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlChaptersDoc.DocumentNode.SelectNodes("//li[@class='wp-manga-chapter    ']");
            if (scrappedChapters != null)
                foreach (var chapter in scrappedChapters)
                {
                   var chapterName = chapter.SelectSingleNode("a").InnerText.Replace(mangaName, "").Replace("Capítulo", "").Trim() ?? "";
                    var chapterUrl = chapter.SelectSingleNode("a").GetAttributeValue("href", "") ?? "";
                    var releasedDate = chapter.SelectSingleNode("span/i").InnerText;
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

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
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
                        mangasList.Add(new MangaSourceDTO
                        {
                            Name = mangaName.Trim(),
                            Url = mangaUrl.Split("/").TakeLast(2).First(),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;
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
