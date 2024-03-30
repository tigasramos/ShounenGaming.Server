using HtmlAgilityPack;
using Newtonsoft.Json;
using Serilog;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class BRMangasScrapper : IBaseMangaScrapper
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CacheHelper _cacheHelper;

        public BRMangasScrapper(IHttpClientFactory httpClientFactory, CacheHelper cacheHelper)
        {
            _httpClientFactory = httpClientFactory;
            _cacheHelper = cacheHelper;
        }

        public async Task<ScrappedManga> GetManga(string urlPart)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync($"https://www.brmangas.net/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='titulo text-uppercase']")?.InnerText.Replace("Ler", "").Trim() ?? "";
            var mangaDescription = htmlDoc.DocumentNode.SelectNodes("//div[@class='serie-texto']/div/p")?[1].InnerText ?? "";
            var imageUrl = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='serie-capa']/img")?.GetAttributeValue("src", "") ?? "";

            var chapters = new List<ScrappedChapter>();
            var scrappedChapters = htmlDoc.DocumentNode.SelectNodes("//li[@class='row lista_ep']/a");
            foreach(var chapter in scrappedChapters) 
            {
                var chapterName = chapter.InnerText.Replace("Capítulo", "") ?? "";
                var chapterUrl = chapter.GetAttributeValue("href", "") ?? "";
                chapters.Add(new ScrappedChapter
                {
                    Name= chapterName.Trim(),
                    Link = chapterUrl.Replace("https://www.brmangas.net/ler/", ""),
                });
            }

            return new ScrappedManga
            {
                Name = HttpUtility.HtmlDecode(mangaName.Remove(mangaName.Length - 6).Trim()),
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
                Source = GetMangaSourceEnumDTO()
            };
        }
        public async Task<List<string>> GetChapterImages(string urlPart)
        {
            return await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.CUSTOM, async _ => 
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Host", "cdn.plaquiz.xyz");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");

                string pattern = @"^(?<name>[\w-]+?)-(?<chapter>(?:\d+-)?\d+(?:\.\d+)?)-?online/$";

                Match match = Regex.Match(urlPart, pattern);

                string name = match.Groups["name"].Value.Trim();
                string chapter = match.Groups["chapter"].Value.Replace("-", ".");

                List<string> imagesUrls = new();
                var lastFound = true;
                var i = 1;
                var extensions = new List<string> { "jpg", "png" };
                while (lastFound)
                {
                    lastFound = false;
                    for (int j = 0; j < extensions.Count && !lastFound; j++)
                    {
                        var url = $"https://cdn.plaquiz.xyz/uploads/{name.First()}/{name}/{chapter}/{i}.{extensions[j]}";
                        try
                        {
                            HttpResponseMessage response = await client.GetAsync(url);
                            if (response.IsSuccessStatusCode)
                            {
                                imagesUrls.Add(url);
                                lastFound = true;
                            }
                        }
                        catch { }
                    }

                    i++;
                }
                return imagesUrls;
            }, urlPart) ?? new List<string>();
        }

        public MangaTranslationEnumDTO GetLanguage()
        {
            return MangaTranslationEnumDTO.PT;
        }

        public string GetBaseURLForManga()
        {
            return "https://www.brmangas.net/ler";
        }

        public MangaSourceEnumDTO GetMangaSourceEnumDTO()
        {
            return MangaSourceEnumDTO.BR_MANGAS;
        }

        public async Task<List<MangaSourceDTO>> SearchManga(string name)
        {
            var web = new HtmlWeb();
            var mangasList = new List<MangaSourceDTO>();
            var currentPage = 1;
            try
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://www.brmangas.net/page/{currentPage}?s={name.Replace(" ", "+")}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listagem row']/div/div/a");
                if (mangasFetched == null || !mangasFetched.Any())
                    return mangasList;

                foreach (var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("h2").InnerText ?? "";

                    if (mangaName.Contains("(Novel)"))
                        continue;
                    var mangaURL = manga.GetAttributeValue("href", "") ?? "";
                    var imageURL = manga.SelectSingleNode("div/img").GetAttributeValue("src", "") ?? "";
                    mangasList.Add(new MangaSourceDTO
                    {
                        Name = HttpUtility.HtmlDecode(mangaName.Trim()),
                        Url = mangaURL.Remove(mangaURL.Length - 1).Split("/").Last(),
                        ImageURL = imageURL,
                        Source = GetMangaSourceEnumDTO()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"BRMangas - SearchManga: {ex.Message}");
            }


            return mangasList;
        }
    }
}
