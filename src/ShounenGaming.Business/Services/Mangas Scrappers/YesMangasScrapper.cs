using HtmlAgilityPack;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    // IMPORTANT: Needs at least 1sec delay between requests
    internal class YesMangasScrapper : IBaseMangaScrapper
    {
        public async Task<List<MangaSourceDTO>> GetAllMangasByPage(int page)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://yesmangas1.com/mangas/page/{page}");

                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='two columns']");
                if (mangasFetched == null || !mangasFetched.Any()) return new List<MangaSourceDTO>();

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("h3/a")?.InnerText ?? "";
                    if (mangaName.Contains("(Novel)")) continue;

                    var mangaUrl = manga.SelectSingleNode("a")?.GetAttributeValue("href", "") ?? "";
                    var imageUrl = manga.SelectSingleNode("a/img").GetAttributeValue("data-path", "") ?? "";
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                        Url = mangaUrl.Remove(mangaUrl.Length - 1).Split("/").Last(),
                        ImageURL = imageUrl,
                        Source = GetMangaSourceEnumDTO()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DiskusScan - GetAllMangasByPage: {ex.Message}");
            }

            return mangasList;
        }

        public string GetBaseURLForManga()
        {
            return "https://yesmangas1.com/manga/";
        }

        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://yesmangas1.com/manga/{urlPart}");

            List<string> imagesUrls = new();
            var images = htmlDoc.DocumentNode.SelectNodes("//div[@class='read-slideshow']/a/img");

            foreach (var image in images)
            {
                imagesUrls.Add(image.GetAttributeValue("src", "").ToString().Trim());
            }
            return imagesUrls;
        }

        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Host = "yesmangas1.com";

            var response = await client.GetAsync($"https://yesmangas1.com/manga/{urlPart}");
            response.EnsureSuccessStatusCode();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync());
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='title']")?.InnerText.Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='content']/div/article")?.InnerText.Trim() ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='descricao']/img")?.GetAttributeValue("data-path", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//div[@id='capitulos']/div/a");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.InnerText.Trim() ?? "";
                var chapterUrl = chapter.GetAttributeValue("href", "") ?? "";
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl.Replace("https://yesmangas1.com/manga/", "")
                });
            }

            return new ScrappedManga
            {
                Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                Description = HttpUtility.HtmlDecode(mangaDescription),
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.YES_MANGAS;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            int currentPage = 1;

            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://yesmangas1.com/search?q={name.Replace(" ", "+")}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//tbody[@id='leituras']/tr");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.SelectSingleNode("td/div/a/h4")?.InnerText ?? "";
                        if (mangaName.Contains("(Novel)")) continue;

                        var mangaUrl = manga.SelectSingleNode("td/a")?.GetAttributeValue("href", "") ?? "";
                        var imageUrl = manga.SelectSingleNode("td/a/img").GetAttributeValue("data-path", "") ?? "";
                        mangasList.Add(new MangaSourceDTO
                        {
                            Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                            Url = mangaUrl.Replace("https://yesmangas1.com/manga/", ""),
                            ImageURL = imageUrl,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"YesMangas - SearchManga: {ex.Message}");
            }

            return mangasList;
        }
    }
}
