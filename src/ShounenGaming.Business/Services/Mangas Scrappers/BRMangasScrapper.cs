using HtmlAgilityPack;
using Newtonsoft.Json;
using Serilog;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Text;
using System.Web;

namespace ShounenGaming.Business.Services.Mangas_Scrappers
{
    public class BRMangasScrapper : IBaseMangaScrapper
    {
      
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
            var web = new HtmlWeb();
            web.OverrideEncoding = Encoding.UTF8;
            var htmlDoc = await web.LoadFromWebAsync($"https://www.brmangas.net/ler/{urlPart}");

            List<string> imagesUrls = new();
            var scripts = htmlDoc.DocumentNode.SelectNodes("//script[@type='text/javascript']");
            var imagesJson = scripts.Where(s => s.InnerText?.Trim().StartsWith("imageArray") ?? false).First().InnerText.Trim();
            var test = imagesJson.Replace("\\", "").Replace("imageArray =", "")[1..^2].Trim();
            var imagesDynamic = JsonConvert.DeserializeObject<ImagesArray>(test);
            var images = imagesDynamic.Images;
            foreach (var image in images)
            {
                imagesUrls.Add(image.Trim());
            }
            return imagesUrls;
        }

        private class ImagesArray
        {
            public List<string> Images { get; set; }
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
                while (true)
                {
                    var htmlDoc = await web.LoadFromWebAsync($"https://www.brmangas.net/page/{currentPage}?s={name.Replace(" ", "+")}");
                    var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listagem row']/div/div/a");
                    if (mangasFetched == null || !mangasFetched.Any()) break;

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
                    currentPage++;
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
