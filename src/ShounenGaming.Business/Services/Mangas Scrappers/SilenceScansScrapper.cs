using HtmlAgilityPack;
using Newtonsoft.Json;
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
    internal class SilenceScansScrapper : IBaseMangaScrapper
    {
        //SilenceScan PT
        //2 seg -> 71
        public async Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            var currentPage = 1;

            try
            {
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://silencescan.com.br/manga/?page={currentPage}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listupd']/div/div/a");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.GetAttributeValue("title", "") ?? "";
                        var mangaURL = manga.GetAttributeValue("href", "") ?? "";
                        var imageURL = manga.SelectSingleNode("div[@class='limit']/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaURL.Remove(mangaURL.Length - 1).Split("/").Last(),
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
            var htmlDoc = await web.LoadFromWebAsync($"https://silencescan.com.br/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='entry-title']").InnerText ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectNodes("//div[@itemprop='description']/p")?[0].InnerText ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='thumb']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//div[@class='eplister']/ul/li/div/div[@class='eph-num']/a");
            foreach (var chapter in scrappedChapters)
            {
                var chapterName = chapter.SelectSingleNode("span[@class='chapternum']").InnerText.Replace("Capítulo", "").Trim() ?? "";
                var chapterUrl = chapter.GetAttributeValue("href", "") ?? "";
                var chapterPageDate = chapter.SelectSingleNode("span[@class='chapterdate']").InnerText;
                var parsed = DateTime.TryParseExact(chapterPageDate, "MMMM d,yyyy", new CultureInfo("pt"), DateTimeStyles.AllowWhiteSpaces, out var chapterDate);
                chapters.Add(new ScrappedChapter
                {
                    Name = chapterName,
                    Link = chapterUrl.Replace("https://silencescan.com.br/", "").Trim(),
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
            web.OverrideEncoding = Encoding.UTF8;
            var htmlDoc = await web.LoadFromWebAsync($"https://silencescan.com.br/{urlPart}");

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

        public string GetBaseURLForManga()
        {
            return "https://silencescan.com.br";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.SILENCE_SCANS;
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
                    var htmlDoc = await web.LoadFromWebAsync($"https://silencescan.com.br/page/{currentPage}/?s={name.Replace(" ", "+")}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listupd']/div/div/a");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

                    foreach (var manga in mangasFetched)
                    {
                        var mangaName = manga.GetAttributeValue("title", "") ?? "";
                        var mangaURL = manga.GetAttributeValue("href", "") ?? "";
                        var imageURL = manga.SelectSingleNode("div[@class='limit']/img").GetAttributeValue("src", "") ?? "";
                        mangasList.Add(new ScrappedSimpleManga
                        {
                            Name = mangaName,
                            Link = mangaURL.Remove(mangaURL.Length - 1).Split("/").Last(),
                            ImageURL = imageURL,
                            Source = GetMangaSourceEnumDTO()
                        });
                    }
                    currentPage++;
                }

            }
            catch(Exception _) { }
           

            return mangasList;
        }
    }
}
