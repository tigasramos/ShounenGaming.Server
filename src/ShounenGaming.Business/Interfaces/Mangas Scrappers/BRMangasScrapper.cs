﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ShounenGaming.Business.Interfaces.Mangas_Scrappers
{
    internal class BRMangasScrapper : IBaseMangaScrapper
    {
        //BRMangas PT
        //1:42 seg -> 5557
        public async  Task<List<ScrappedSimpleManga>> GetAllMangas()
        {
            var web = new HtmlWeb();
            var mangasList = new List<ScrappedSimpleManga>();
            var currentPage = 1;
            while (true)
            {
                var htmlDoc = await web.LoadFromWebAsync($"https://www.brmangas.net/lista-de-manga/page/{currentPage}");
                var mangasFetched = htmlDoc.DocumentNode.SelectNodes("//div[@class='listagem row']/div/div/a");
                if (mangasFetched == null)
                    break;
                foreach(var manga in mangasFetched)
                {
                    var mangaName = manga.SelectSingleNode("h2").InnerText ?? "";
                    var mangaURL = manga.GetAttributeValue("href", "") ?? "";
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
            var htmlDoc = await web.LoadFromWebAsync($"https://www.brmangas.net/manga/{urlPart}");
            var mangaName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='titulo text-uppercase']")?.InnerText.Replace("Ler", "").Replace("Online", "").Trim() ?? "";
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
                Name = mangaName,
                Description = mangaDescription,
                Chapters = chapters,
                ImageURL = imageUrl,
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
    }
}
