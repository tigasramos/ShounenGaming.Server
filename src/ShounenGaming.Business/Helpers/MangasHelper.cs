using AutoMapper;
using ShounenGaming.Business.Services.Mangas_Scrappers;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.DTOs.Models.Mangas;
using Serilog;
using static ShounenGaming.Business.Helpers.CacheHelper;
using System.Net;
using JikanDotNet;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Exceptions;

namespace ShounenGaming.Business.Helpers
{
    public class MangasHelper
    {
        #region Static
        public static bool IsMangaNSFW(IEnumerable<string> tags)
        {
            // Add More Tags if Needed
            var nsfwTags = new List<string>() 
            { 
                "erotica", "hentai"
            };

            return tags.Any(t => nsfwTags.Contains(t.ToLowerInvariant()));
        }

        public static List<JikanDotNet.Manga> FilterCorrectTypes(ICollection<JikanDotNet.Manga> list)
        {
            return list.Where(IsMALMangaCorrectType).ToList();
        }

        public static bool IsMALMangaCorrectType(JikanDotNet.Manga m)
        {
            return m.Type.ToLowerInvariant() != "Light Novel".ToLowerInvariant() &&
                   m.Type.ToLowerInvariant() != "Novel".ToLowerInvariant() &&
                   m.Type.ToLowerInvariant() != "One-shot".ToLowerInvariant();
        }
        public static bool IsALMangaCorrectType(AniListHelper.ALManga m)
        {
            return m.Type.ToLowerInvariant() != "NOVEL".ToLowerInvariant() &&
                   m.Type.ToLowerInvariant() != "ONE_SHOT".ToLowerInvariant();
        }

        public static MangaTypeEnum ConvertMALMangaType(string mangaType)
        {
            return mangaType.ToLower() switch
            {
                "manga" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANGA,
                "manhua" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHUA,
                "manhwa" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHWA,
                "cn" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHUA,
                "kr" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANHWA,
                "jp" => Core.Entities.Mangas.Enums.MangaTypeEnum.MANGA,
                _ => Core.Entities.Mangas.Enums.MangaTypeEnum.MANGA,
            };
        }
        public static string BuildTranslationFolderPath(string mangaNameNormalized, string translation, string chapterNameNormalized)
        {
            return $"mangas/{mangaNameNormalized}/chapters/{translation}/{chapterNameNormalized}";
        }
        #endregion

        private readonly IMapper _mapper;
        private readonly IEnumerable<IBaseMangaScrapper> _scrappers;

        private readonly IJikan _jikan;
        private readonly IMangaRepository _mangaRepo;
        private readonly IMangaWriterRepository _mangaWriterRepo;
        private readonly IMangaTagRepository _mangaTagRepo;
        private readonly IAddedMangaActionRepository _addedMangaRepo;

        private readonly IImageService _imageService;
        private readonly CacheHelper _cacheHelper;

        public MangasHelper(IEnumerable<IBaseMangaScrapper> scrappers, IMapper mapper, IJikan jikan, IMangaRepository mangaRepo, IMangaWriterRepository mangaWriterRepo, IMangaTagRepository mangaTagRepo, IAddedMangaActionRepository addedMangaRepo, IImageService imageService, CacheHelper cacheHelper)
        {
            _scrappers = scrappers;
            _mapper = mapper;
            _jikan = jikan;
            _mangaRepo = mangaRepo;
            _mangaWriterRepo = mangaWriterRepo;
            _mangaTagRepo = mangaTagRepo;
            _addedMangaRepo = addedMangaRepo;
            _imageService = imageService;
            _cacheHelper = cacheHelper;
        }

        public async Task DownloadImagesFromManga(Core.Entities.Mangas.Manga manga)
        {
            var downloadedManga = await _imageService.GetAllMangaChapters(manga.Name.NormalizeStringToDirectory());
            var downloadedChapters = new List<string>();

            // Download Images per sources
            foreach (var source in manga.Sources)
            {
                Log.Information($"Downloading Images for Source: {source.Name}");
                try
                {
                    // Get Scrapper
                    var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                    var scrapper = _scrappers.First(s => s.GetMangaSourceEnumDTO() == scrapperEnum);
                    if (scrapper.GetLanguage() != MangaTranslationEnumDTO.PT) continue;

                    // Search Manga from Scrapper
                    var fetchedManga = await scrapper.GetManga(source.Url);
                    if (fetchedManga.Chapters == null || fetchedManga.Chapters.Count == 0) continue;

                    // Select Chapters to Download
                    var chapters = manga.Chapters.Where(c => c.Translations.Any(t => t.Language == TranslationLanguageEnum.PT));
                    foreach (var chapter in chapters)
                    {
                        try
                        {
                            var fetchedChapter = fetchedManga.Chapters
                                .First(c => c.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",", ".").Trim() ==
                                chapter.Name.ToString().Replace(",", "."));

                            Log.Information($"Checking Chapter: {fetchedChapter.Name}");

                            // Filter out already downloaded (if not forced)
                            var downloadedChapterData = downloadedManga?.Chapters.FirstOrDefault(c => c.Name == fetchedChapter.Name);
                            if (downloadedChapters.Contains(fetchedChapter.Name))
                                continue;

                            Log.Information($"Downloading Chapter: {fetchedChapter.Name}");

                            downloadedChapters.Add(fetchedChapter.Name);
                            var translation = chapter.Translations.First(t => t.Language == TranslationLanguageEnum.PT);
                            if (await SaveImage(scrapper, TranslationLanguageEnum.PT, manga.Name, chapter.Name.ToString(), fetchedChapter.Link, downloadedChapterData?.Pages))
                            {
                                translation.IsWorking = true;
                                await Task.Delay(2000);
                            }

                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Problem saving Image");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Problem saving Image with Source");
                }
            }

            await _mangaRepo.Update(manga);
        }
        private async Task<bool> SaveImage(IBaseMangaScrapper scrapper, TranslationLanguageEnum scrapperTranslation, string mangaName, string chapterName, string chapterLink, int? alreadyDownloadedPages = null)
        {
            try
            {
                var mangaNameSimplified = mangaName.NormalizeStringToDirectory();
                var folderPath = MangasHelper.BuildTranslationFolderPath(mangaNameSimplified, scrapperTranslation.ToString().ToLower(), chapterName.NormalizeStringToDirectory());
                
                var chapterPages = await scrapper.GetChapterImages(chapterLink);

                // Already has those pages
                if (alreadyDownloadedPages != null && alreadyDownloadedPages >= chapterPages.Count) return true;

                for (int i = 0; i < chapterPages.Count; i++)
                {
                    using WebClient webClient = new();
                    webClient.Headers.Add("user-agent", "User Agent");
                    var page = chapterPages[i];
                    webClient.Headers.Add("referer", $"{scrapper.GetBaseURLForManga()}/{chapterLink}");

                    var data = webClient.DownloadData(page);
                    await _imageService.SaveImage(data, $"{folderPath}/{i}.{page.Split(".").Last()}");
                    await Task.Delay(200);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new SavingImageException(ex.Message);
            }
        }

        public async Task DownloadImagesFromMangaChapter(Core.Entities.Mangas.MangaChapter chapter)
        {
            foreach (var source in chapter.Manga.Sources)
            {
                try
                {
                    // Get Scrapper
                    var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                    var scrapper = _scrappers.First(s => s.GetMangaSourceEnumDTO() == scrapperEnum);
                    if (scrapper.GetLanguage() != MangaTranslationEnumDTO.PT) continue;

                    // Search Manga from Scrapper
                    var fetchedManga = await scrapper.GetManga(source.Url);
                    if (fetchedManga.Chapters == null || fetchedManga.Chapters.Count == 0) throw new Exception("Chapter Not Found");
                    try
                    {
                        var fetchedChapter = fetchedManga.Chapters
                            .First(c => c.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",", ".").Trim() ==
                            chapter.Name.ToString().Replace(",", "."));


                        var translation = chapter.Translations.First(t => t.Language == TranslationLanguageEnum.PT);
                        if (await SaveImage(scrapper, TranslationLanguageEnum.PT, chapter.Manga.Name, chapter.Name.ToString(), fetchedChapter.Link))
                        {
                            translation.IsWorking = true;
                            await Task.Delay(2000);
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Problem saving Image");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Problem saving Image with Source");
                }
            }

            await _mangaRepo.Update(chapter.Manga);
        }

        #region Add Manga

        /// <summary>
        /// Adds a new MAL Manga to the DB, if already exists just returns it
        /// </summary>
        /// <param name="malId">MyAnimeList Id</param>
        /// <returns></returns>
        public async Task<Core.Entities.Mangas.Manga> AddMALManga(long malId, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByMALId(malId);
            if (dbManga != null) return dbManga;

            var mangaResponse = await _jikan.GetMangaAsync(malId);
            var manga = mangaResponse.Data;

            return await AddMALManga(manga, userId);
        }
        public async Task<Core.Entities.Mangas.Manga> AddMALManga(JikanDotNet.Manga manga, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByMALId(manga.MalId);
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

            var images = new List<string>
            {
                manga.Images.JPG.ImageUrl
            };

            long? aniListId = null;
            var synonyms = new List<string>();
            int? anilistPopularity = null;
            int? anilistAverageScore = null;
            // Try to get Anilist Id aswell
            try
            {
                var aniListManga = await AniListHelper.GetMangaByMALId(manga.MalId);
                aniListId = aniListManga.Id;
                synonyms = aniListManga.Synonyms;
                anilistPopularity = aniListManga.Popularity;
                anilistAverageScore = aniListManga.AverageScore ?? aniListManga.MeanScore;
                if (!IsALMangaCorrectType(aniListManga))
                    return null;

                if (!string.IsNullOrEmpty(aniListManga.CoverImage.Large))
                    images.Add(aniListManga.CoverImage.Large);
            }
            catch { }


            return await SaveMangaInDb(new Core.Entities.Mangas.Manga
            {
                MangaMyAnimeListID = manga.MalId,
                MangaAniListID = aniListId,
                Name = manga.Titles.FirstOrDefault(c => c.Type == "Default")?.Title ?? manga.Title,
                AlternativeNames = manga.Titles.Select(t => new MangaAlternativeName { Language = t.Type, Name = t.Title }).ToList(),
                Synonyms = synonyms.Select(d => new MangaSynonym { Name = d }).ToList(),
                IsReleasing = manga.Publishing,
                Type = ConvertMALMangaType(manga.Type),
                Description = manga.Synopsis,
                Writer = writer,
                Tags = tags,
                ImagesUrls = images,
                IsNSFW = IsMangaNSFW(tags.Select(t => t.Name)),
                MALPopularity = manga.Members,
                MALScore = manga.Score.HasValue ? decimal.ToDouble(manga.Score.Value) : null,
                ALPopularity = anilistPopularity,
                ALScore = anilistAverageScore / (double)10,
                StartedAt = manga.Published.From is not null ? DateTime.SpecifyKind(manga.Published.From.Value, DateTimeKind.Utc) : null,
                FinishedAt = manga.Published.To is not null ? DateTime.SpecifyKind(manga.Published.To.Value, DateTimeKind.Utc) : null,
            }, userId);
        }


        /// <summary>
        /// Adds a new AL Manga to the DB, if already exists just returns it
        /// </summary>
        /// <param name="mangaId">Anilist Id</param>
        /// <returns></returns>
        public async Task<Core.Entities.Mangas.Manga> AddALManga(long mangaId, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByALId(mangaId);
            if (dbManga != null) return dbManga;


            var manga = await AniListHelper.GetMangaById(mangaId);
            return await AddALManga(manga, userId);
        }
        public async Task<Core.Entities.Mangas.Manga> AddALManga(AniListHelper.ALManga manga, int? userId)
        {
            // Check if already exists
            var dbManga = await _mangaRepo.GetByALId(manga.Id);
            if (dbManga != null) return dbManga;

            //If exists in MAL its preferred
            if (manga.IdMal != null)
            {
                return await AddMALManga(manga.IdMal.Value, userId);
            }


            // Get Writer or Create one
            var firstAuthor = manga.Staff.Edges.FirstOrDefault(s => s.Role.ToLower().Contains("original story"));
            var secondAuthor = manga.Staff.Edges.First(s => s.Role.ToLower().Contains("story"));
            var author = firstAuthor ?? secondAuthor;
            var authorData = manga.Staff.Nodes.Where(n => n.Id == author.Node.Id).First();
            var writer = await _mangaWriterRepo.GetWriterByName(authorData.Name.Full);
            writer ??= new MangaWriter
            {
                Name = authorData.Name.Full,
            };

            // Get Tags
            var tags = new List<MangaTag>();
            var mangaGenres = manga.Genres;
            foreach (var genre in mangaGenres)
            {
                var tag = await _mangaTagRepo.GetTagByName(genre);
                tag ??= new MangaTag
                {
                    Name = genre,
                };

                tags.Add(tag);
            }

            var alternativeNames = new List<MangaAlternativeName>();

            if (!string.IsNullOrEmpty(manga.Title.English))
                alternativeNames.Add(new MangaAlternativeName
                {
                    Language = "English",
                    Name = manga.Title.English
                });
            if (!string.IsNullOrEmpty(manga.Title.Native))
                alternativeNames.Add(new MangaAlternativeName
                {
                    Language = "Native",
                    Name = manga.Title.Native

                });
            if (!string.IsNullOrEmpty(manga.Title.Romaji))
                alternativeNames.Add(new MangaAlternativeName
                {
                    Language = "Romaji",
                    Name = manga.Title.Romaji
                });

            var type = ConvertMALMangaType(manga.CountryOfOrigin);
            return await SaveMangaInDb(new Core.Entities.Mangas.Manga
            {
                MangaAniListID = manga.Id,
                MangaMyAnimeListID = manga.IdMal,
                Name = type != MangaTypeEnum.MANGA && !string.IsNullOrEmpty(manga.Title.English) ? manga.Title.English : manga.Title.UserPreferred,
                AlternativeNames = alternativeNames,
                Synonyms = manga.Synonyms.Select(d => new MangaSynonym { Name = d }).ToList(),
                IsReleasing = manga.Status == "RELEASING",
                Type = type,
                Description = manga.Description,
                Writer = writer,
                Tags = tags,
                IsNSFW = IsMangaNSFW(tags.Select(t => t.Name)),
                ALPopularity = manga.Popularity,
                ALScore = manga.AverageScore is not null ? manga.AverageScore / (double)10 : manga.MeanScore / (double)10,
                ImagesUrls = new List<string> { manga.CoverImage.Large },
                StartedAt = manga.StartDate.Year is not null ? DateTime.SpecifyKind(new DateTime(manga.StartDate.Year.Value, manga.StartDate.Month ?? 1, manga.StartDate.Day ?? 1), DateTimeKind.Utc) : null,
                FinishedAt = manga.EndDate.Year is not null ? DateTime.SpecifyKind(new DateTime(manga.EndDate.Year.Value, manga.EndDate.Month ?? 1, manga.EndDate.Day ?? 1), DateTimeKind.Utc) : null,
            }, userId);
        }

        private async Task<Core.Entities.Mangas.Manga> SaveMangaInDb(Core.Entities.Mangas.Manga manga, int? userId)
        {
            var dbManga = await _mangaRepo.Create(manga);
            Log.Information($"Added new Manga: {manga.Name}");

            // If a Users adds the Manga
            if (userId.HasValue)
            {
                await _addedMangaRepo.Create(new AddedMangaAction
                {
                    UserId = userId.Value,
                    Manga = dbManga,
                });
                await _cacheHelper.DeleteCache(CacheKey.ADD_MANGA_ACTION);
            }


            // Save Thumbnail Image
            try
            {
                using WebClient webClient = new();
                var mangaNameSimplified = dbManga.Name.NormalizeStringToDirectory();
                webClient.Headers.Add("user-agent", "User Agent");
                var url = manga.ImagesUrls.First();
                var data = webClient.DownloadData(url);
                await _imageService.SaveImage(data, $"mangas/{mangaNameSimplified}/thumbnail.{url.Split(".").Last()}");
            }
            catch
            {
                Log.Error($"Error saving thumbnail image for: {dbManga.Name}");
            }

            return dbManga;
        }
        #endregion


        public MangaTranslationDTO MapMangaTranslation(MangaTranslation mangaTranslation, string source, List<string> pages, bool changeTranslation)
        {
            var dto = new MangaTranslationDTO
            {
                MangaName = mangaTranslation.MangaChapter.Manga.Name,
                MangaType = _mapper.Map<MangaTypeEnumDTO>(mangaTranslation.MangaChapter.Manga.Type),
                Language = _mapper.Map<MangaTranslationEnumDTO>(mangaTranslation.Language),
                ChapterId = mangaTranslation.MangaChapter.Id,
                ChapterNumber = mangaTranslation.MangaChapter.Name,
                ReleasedAt = mangaTranslation.ReleasedDate,
                Source = source,
                Pages = pages,
                PageHeaders = string.IsNullOrEmpty(source) ? new Dictionary<string, string>() : GetScrapperByEnum(Enum.Parse<MangaSourceEnumDTO>(source))?.GetImageHeaders(),
                CreatedAt = mangaTranslation.CreatedAt,
            };
            var previousChapter = mangaTranslation.MangaChapter.Manga.Chapters.OrderByDescending(o => o.Name).SkipWhile(s => s.Id != mangaTranslation.MangaChapter.Id).Skip(1).Take(1).FirstOrDefault();
            dto.PreviousChapterId = (changeTranslation || (previousChapter?.Translations.Any(t => t.Language == mangaTranslation.Language) ?? false)) ? previousChapter?.Id : null;

            var nextChapter = mangaTranslation.MangaChapter.Manga.Chapters.OrderBy(o => o.Name).SkipWhile(s => s.Id != mangaTranslation.MangaChapter.Id).Skip(1).Take(1).FirstOrDefault();
            dto.NextChapterId = (changeTranslation || (nextChapter?.Translations.Any(t => t.Language == mangaTranslation.Language) ?? false)) ? nextChapter?.Id : null;
            return dto;
        }
        public IBaseMangaScrapper? GetScrapperByEnum(MangaSourceEnumDTO source)
        {
            return _scrappers.FirstOrDefault(s => s.GetMangaSourceEnumDTO() == source);
        }
    }
}
