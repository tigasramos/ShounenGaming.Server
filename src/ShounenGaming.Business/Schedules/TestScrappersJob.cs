using Coravel.Invocable;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Services.Mangas_Scrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Schedules
{
    public class TestScrappersJob : IInvocable
    {
        private readonly IServiceProvider services;

        public TestScrappersJob(IServiceProvider services)
        {
            this.services = services;
        }


        public async Task Invoke()
        {
            try
            {
                using var scope = services.CreateScope();
                var scrappers = scope.ServiceProvider.GetRequiredService<IEnumerable<IBaseMangaScrapper>>();
                foreach (var scrapper in scrappers)
                {
                    Log.Information($"Testing {scrapper.GetType()}");
                    try
                    {

                        var mangas = await scrapper.SearchManga("a");

                        if (mangas.Count > 0)
                            Log.Information($"SearchManga: Checked");
                        else
                            Log.Information($"SearchManga: Failed");
                        

                        foreach(var mangaInfo in mangas)
                        {
                            var manga = await scrapper.GetManga(mangaInfo.Url);
                            if (manga.Chapters.Any())
                            {
                                Log.Information($"GetManga: Checked");
                                var images = await scrapper.GetChapterImages(manga.Chapters.First().Link);

                                if (images.Any() && images.Count > 5)
                                    Log.Information($"GetChapterImages: Checked");
                                else
                                    Log.Information($"GetChapterImages: Failed");
                                
                                break;
                            }
                            else
                            {
                                Log.Information($"GetManga: Failed");
                            }
                        }
                    } 
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message + "\n" + ex.StackTrace);
                    }

                    await Task.Delay(1000);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Fetching Season Mangas");
            }
        }
    }
}
