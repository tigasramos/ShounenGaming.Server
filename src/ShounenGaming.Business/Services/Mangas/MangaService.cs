using AutoMapper;
using JikanDotNet;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers;
using ShounenGaming.Business.Interfaces.Mangas_Scrappers.Models;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.Business.Models.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System.Net;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaService : IMangaService
    {
        //Order by Priority (Scans first then remaining)
        static List<IBaseMangaScrapper> scrappers = new() {
                new MangasChanScrapper(), new ManganatoScrapper(),
                new GekkouScansScrapper(), new SilenceScansScrapper(),
                new HuntersScansScrapper(), new NeoXScansScrapper(),
                /*new FireMangasScrapper(),*/ new BRMangasScrapper() };

        private readonly IMangaRepository _mangaRepo;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IMangaWriterRepository _mangaWriterRepo;
        private readonly IMangaTagRepository _mangaTagRepo;

        private readonly IJikan _jikan;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public MangaService(IMangaRepository mangaRepo, IMangaUserDataRepository mangaUserDataRepo, IMangaWriterRepository mangaWriterRepo, IMangaTagRepository mangaTagRepo, IMapper mapper, IImageService imageService, IJikan jikan)
        {
            _mangaRepo = mangaRepo;
            _mangaUserDataRepo = mangaUserDataRepo;
            _mangaWriterRepo = mangaWriterRepo;
            _mangaTagRepo = mangaTagRepo;
            _mapper = mapper;
            _imageService = imageService;
            _jikan = jikan;
        }


        public async Task<MangaDTO> GetMangaById(int id)
        {
            var manga = await _mangaRepo.GetById(id);
            if (manga == null)
                throw new EntityNotFoundException("Manga");
            return _mapper.Map<MangaDTO>(manga);
        }

        public async Task<PaginatedResponse<MangaDTO>> SearchMangaByName(string name)
        {
            var mangas = await _mangaRepo.SearchMangaByName(name);
            throw new NotImplementedException();
        }


        public async Task<List<MangaInfoDTO>> GetPopularMangas()
        {
            var popularMangas = await _mangaRepo.GetPopularMangas(5);
            if (popularMangas == null)
                throw new Exception("Error Fetching Popular Mangas");

            if (popularMangas.Count < 5)
            {
                //TODO: Think about it
                popularMangas.AddRange((await _mangaRepo.GetAll()).Take(5 - popularMangas.Count).ToList());
            }

            return _mapper.Map<List<MangaInfoDTO>>(popularMangas);
        }

        public async Task<List<MangaInfoDTO>> GetMangasByStatusByUser(int userId, MangaUserStatusEnumDTO status)
        {
            var mangas = await _mangaUserDataRepo.GetByStatusByUser(_mapper.Map<MangaUserStatusEnum>(status), userId);
            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }

        public async Task<List<MangaInfoDTO>> GetRecentlyAddedMangas()
        {
            var mangas = await _mangaRepo.GetRecentlyAddedMangas();
            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }

        public async Task<List<ChapterReleaseDTO>> GetRecentlyReleasedChapters()
        {
            var chapters = await _mangaRepo.GetRecentlyReleasedChapters();
            return _mapper.Map<List<ChapterReleaseDTO>>(chapters);
        }

        public async Task<List<MangaWriterDTO>> GetMangaWriters()
        {
            var writers = await _mangaWriterRepo.GetAll();
            return _mapper.Map<List<MangaWriterDTO>>(writers);
        }

        public async Task<List<string>> GetMangaTags()
        {
            var tags = await _mangaTagRepo.GetAll();
            return tags.Select(t => t.Name).ToList();
        }

        public async Task<List<MangaDTO>> GetAll()
        {
            var mangas = await _mangaRepo.GetAll();
            return _mapper.Map<List<MangaDTO>>(mangas);
        }

        public async Task<List<ScrappedSimpleManga>> SearchMangaSource(string name)
        {
            var allMangas = new List<ScrappedSimpleManga>();

            foreach(var scrapper in scrappers)
            {
                allMangas.AddRange(await scrapper.SearchManga(name));
            }

            return allMangas;
        }

        public async Task<List<ScrappedSimpleManga>> GetAllMangasFromSource(MangaSourceEnumDTO source)
        {
            var scrapper = scrappers.Where(s => s.GetMangaSourceEnumDTO() == source).FirstOrDefault();
            var mangas = await scrapper?.GetAllMangas();
            return mangas ?? new List<ScrappedSimpleManga>();
        }

        public async Task<MangaDTO> LinkSourcesToManga(int myAnimeListMangaId, List<ScrappedSimpleManga> mangas)
        {
            //TODO: Handle Exceptions (Timeout and wrong id)
            var manga = await AddManga(myAnimeListMangaId);

            manga.Sources = mangas.Select(m => new MangaSource { BrokenLink = false, Provider = m.Source.ToString(), URL = m.Link }).ToList();

            return _mapper.Map<MangaDTO>(await _mangaRepo.Update(manga));
        }

        public async Task<List<JikanDotNet.Manga>> SearchMangaMetaData(string name)
        {
            return (await _jikan.SearchMangaAsync(name)).Data.ToList();
        }

        public async Task UpdateMangasChapters()
        {
            Log.Information("Starting the Manga Chapters Update");

            var mangas = await _mangaRepo.GetAll();

            foreach(var manga in mangas)
            {
                Log.Information($"Scrapping: {manga.Name}");

                try
                {
                    var sources = manga.Sources.Where(m => !m.BrokenLink);
                    foreach(var source in sources)
                    {
                        try
                        {
                            var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Provider);
                            var scrapper = GetScrapperByEnum(scrapperEnum);
                            var scrapperTranslation = (TranslationLanguage)Enum.Parse(typeof(TranslationLanguage), scrapper.GetLanguage());

                            var mangaInfo = await scrapper.GetManga(source.URL);

                            foreach (var chapter in mangaInfo.Chapters)
                            {
                                var dbChapter = manga.Chapters.FirstOrDefault(c => c.Name == chapter.Name);

                                if (dbChapter is null)
                                {
                                    //TODO: Send to Discord Bot to notify new Chapter Release
                                    dbChapter = new MangaChapter
                                    {
                                        Name = chapter.Name,
                                    };
                                    manga.Chapters.Add(dbChapter);
                                }

                                if (!dbChapter.Translations.Any(t => t.Language == scrapperTranslation))
                                {
                                    dbChapter.Translations.Add(new MangaTranslation
                                    {
                                        ReleasedDate = chapter.ReleasedAt,
                                        Language = scrapperTranslation,
                                    });

                                    await SaveImage(scrapper, scrapperTranslation, manga.Name, dbChapter.Name, chapter.Link);
                                    await _mangaRepo.Update(manga);
                                }


                                await Task.Delay(5000);
                            }
                        } 
                        catch(Exception ex)
                        {
                            source.BrokenLink = true;
                            await _mangaRepo.Update(manga);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error($"Error Scrapping: {manga.Name} : {ex.Message}");
                }
            }

        }

        /// <summary>
        /// Adds a new Manga to the DB, if already exists just returns it
        /// </summary>
        /// <param name="malId">MyAnimeList Id</param>
        /// <returns></returns>
        private async Task<Core.Entities.Mangas.Manga> AddManga(long malId)
        {
            var mangaResponse = await _jikan.GetMangaAsync(malId);
            var manga = mangaResponse.Data;

            // Check if already exists
            var dbManga = await _mangaRepo.GetByMALId(malId);
            if (dbManga != null) return dbManga;

            // Get Writer or Create one
            var author = manga.Authors.First();
            var writer = await _mangaWriterRepo.GetWriterByName(author.Name);
            writer ??= new MangaWriter
            {
                Name = author.Name,
            };

            // Get Tags
            var tags = new List<MangaTag>();
            var mangaGenres = manga.Genres;
            foreach (var genre in mangaGenres)
            {
                var tag = await _mangaTagRepo.GetTagByName(genre.Name);
                tag ??= new MangaTag
                {
                    Name = genre.Name,
                };

                tags.Add(tag);
            }

            dbManga = await _mangaRepo.Create(new Core.Entities.Mangas.Manga
            {
                MangaMyAnimeListID = malId,
                Name = manga.Titles.FirstOrDefault(c => c.Type == "Default")?.Title ?? manga.Title,
                AlternativeNames = manga.Titles.Select(t => new MangaAlternativeName { Language = t.Type, Name = t.Title }).ToList(),
                IsReleasing = manga.Publishing,
                Type = ConvertMALMangaType(manga.Type),
                Description = manga.Synopsis,
                Writer = writer,
                Tags = tags,
                StartedAt = manga.Published.From,
                FinishedAt = manga.Published.To
            });

            // Save Image aswell
            using WebClient webClient = new();
            var mangaNameSimplified = dbManga.Name.NormalizeStringToDirectory();
            webClient.Headers.Add("user-agent", "User Agent");
            var url = manga.Images.JPG.ImageUrl;
            var data = webClient.DownloadData(url);
            await _imageService.SaveImage(data, $"mangas/{mangaNameSimplified}/thumbnail.{url.Split(".").Last()}");

            return dbManga;
        }
       
        
        private async Task SaveImage(IBaseMangaScrapper scrapper, TranslationLanguage scrapperTranslation, string mangaName, string chapterName, string chapterLink)
        {
            var chapterPages = await scrapper.GetChapterImages(chapterLink);

            //Check if images are working, if so, add them to filedata
            using (WebClient webClient = new())
            {
                var mangaNameSimplified = mangaName.NormalizeStringToDirectory();
                webClient.Headers.Add("user-agent", "User Agent");
                for (int i = 0; i < chapterPages.Count; i++)
                {
                    var page = chapterPages[i];
                    webClient.Headers.Add("referer", $"{scrapper.GetBaseURLForManga()}/{chapterLink}");
                    var data = webClient.DownloadData(page);
                    await _imageService.SaveImage(data, $"mangas/{mangaNameSimplified}/chapters/{scrapperTranslation.ToString().ToLower()}/{chapterName.NormalizeStringToDirectory()}/{i}.{page.Split(".").Last()}");
                }
            }
        }
        private static Core.Entities.Mangas.Enums.MangaType ConvertMALMangaType(string mangaType)
        {
            return mangaType.ToLower() switch
            {
                "manga" => Core.Entities.Mangas.Enums.MangaType.MANGA,
                "manhua" => Core.Entities.Mangas.Enums.MangaType.MANHUA,
                "manhwa" => Core.Entities.Mangas.Enums.MangaType.MANHWA,
                _ => throw new Exception(),
            };
        }

        private static IBaseMangaScrapper? GetScrapperByEnum(MangaSourceEnumDTO source)
        {
            return scrappers.FirstOrDefault(s => s.GetMangaSourceEnumDTO() == source);
        }


        public Task<PaginatedResponse<MangaDTO>> SearchMangaByTag(string tag)
        {
            throw new NotImplementedException();
        }

        public Task<MangaWriterDTO> GetMangaWriterById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
