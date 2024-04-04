using JikanDotNet;
using Serilog;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.Core.Entities.Mangas;
using AutoMapper;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Globalization;
using ShounenGaming.Business.Services.Mangas_Scrappers;
using ShounenGaming.Business.Exceptions;
using System.Net;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.Business.Services.Mangas_Scrappers.Models;
using ShounenGaming.Business.Hubs;
using Microsoft.AspNetCore.SignalR;
using static ShounenGaming.Business.Helpers.CacheHelper;
using ShounenGaming.Business.Interfaces.Mangas;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaJobsService : IMangaJobsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMangaRepository _mangaRepo;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IMangaTagRepository _mangaTagRepo;

        private readonly IJikan _jikan;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        private readonly IEnumerable<IBaseMangaScrapper> _scrappers;

        private readonly IHubContext<MangasHub, IMangasHubClient> _mangasHub;
        private readonly IFetchMangasQueue _queue;
        private readonly CacheHelper _cacheHelper;
        private readonly MangasHelper _mangasHelper;

        public MangaJobsService(IUserRepository userRepository, IMangaRepository mangaRepo, IMangaTagRepository mangaTagRepo, IJikan jikan, IImageService imageService, IMapper mapper, IEnumerable<IBaseMangaScrapper> scrappers, IHubContext<MangasHub, IMangasHubClient> mangasHub, IFetchMangasQueue queue, CacheHelper cacheHelper, MangasHelper mangasHelper, IMangaUserDataRepository mangaUserDataRepo)
        {
            _userRepository = userRepository;
            _mangaRepo = mangaRepo;
            _mangaTagRepo = mangaTagRepo;
            _jikan = jikan;
            _imageService = imageService;
            _mapper = mapper;
            _scrappers = scrappers;
            _mangasHub = mangasHub;
            _queue = queue;
            _cacheHelper = cacheHelper;
            _mangasHelper = mangasHelper;
            _mangaUserDataRepo = mangaUserDataRepo;
        }

        #region Top Mangas Job
        public async Task AddOrUpdateAllMangasMetadata()
        {
            Log.Information($"Started Updating Mangas Metadata");

            // Update Mangas Metadata
            var updatedMangas = await UpdateAllMangasMetadata();
            Log.Information($"Updated {updatedMangas} mangas metadata");

            Log.Information("Waiting");
            await Task.Delay(60000);


            Log.Information("Started Adding Popular Mangas");

            // MAL Mangas
            await AddTopMALMangas();

            Log.Information("Waiting");
            await Task.Delay(60000);

            // AL Mangas
            await AddTopALMangas();

            Log.Information("Finished Adding Popular Mangas");
        }
        private async Task AddTopALMangas()
        {
            Log.Information("AniList Mangas");
            var topMangasAL = await AniListHelper.GetPopularMangas();
            foreach (var manga in topMangasAL)
            {
                await _mangasHelper.AddALManga(manga.Id, null);
            }

            Log.Information("Waiting");
            await Task.Delay(60000);

            Log.Information("AniList Manhwas");
            var topKRMangasAL = await AniListHelper.GetPopularManhwas();
            foreach (var manga in topKRMangasAL)
            {
                await _mangasHelper.AddALManga(manga, null);
            }

            Log.Information("Waiting");
            await Task.Delay(60000);

            Log.Information("AniList Manhuas");
            var topCHMangasAL = await AniListHelper.GetPopularManhuas();
            foreach (var manga in topCHMangasAL)
            {
                await _mangasHelper.AddALManga(manga, null);
            }
        }
        private async Task AddTopMALMangas()
        {
            Log.Information("MyAnimeList Mangas");
            for (int i = 1; i < 7; i++)
            {
                var topMangasMAL = await _jikan.GetTopMangaAsync(i);
                var malList = MangasHelper.FilterCorrectTypes(topMangasMAL.Data);
                foreach (var manga in malList)
                {
                    await _mangasHelper.AddMALManga(manga, null);
                }
            }
        }
        #endregion

        #region Season Mangas
        public async Task AddNewSeasonMangas() 
        {
            Log.Information("Reseting Season Mangas");

            var allMangas = await _mangaRepo.GetAll();

            // Remove old season mangas
            foreach (var manga in allMangas)
            {
                if (!manga.IsSeasonManga) continue;
                manga.IsSeasonManga = false;
                await _mangaRepo.Update(manga);
            }

            Log.Information("Getting Season Mangas");
            var seasonAnimes = await _jikan.GetCurrentSeasonAsync();
            await Task.Delay(1000);

            foreach (var anime in seasonAnimes.Data)
            {
                try
                {
                    if (anime is null)
                        continue;

                    await AddSeasonManga(anime, allMangas);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error Fetching {anime.Titles.First().Title}", ex);
                }

            }

            await _cacheHelper.DeleteCache(CacheKey.SEASON_MANGAS);
            Log.Information("Season Mangas Updated");
        }
        private async Task AddSeasonManga(Anime anime, IList<Core.Entities.Mangas.Manga> allMangas)
        {
            var relations = await _jikan.GetAnimeRelationsAsync(anime.MalId!.Value);
            var mangaId = relations.Data.Where(r => r.Relation == "Adaptation" && r.Entry.First().Type == "manga").Select(s => s.Entry.First().MalId).First();

            var dbManga = allMangas.Where(m => m.MangaMyAnimeListID == mangaId).FirstOrDefault();
            BaseJikanResponse<JikanDotNet.Manga>? manga = null;
            if (dbManga is null)
            {
                await Task.Delay(350);

                // Fetch First Degree Relation
                manga = await _jikan.GetMangaAsync(mangaId);
                if (manga is null)
                    return;

                var isMangaType = MangasHelper.IsMALMangaCorrectType(manga!.Data);
                if (!isMangaType)
                {
                    await Task.Delay(350);

                    // Fetch Second Degree Relation
                    var relationsManga = await _jikan.GetMangaRelationsAsync(mangaId);
                    mangaId = relationsManga.Data.Where(r => r.Relation == "Alternative version" && r.Entry.First().Type == "manga").Select(s => s.Entry.First().MalId).First();

                    dbManga = allMangas.Where(m => m.MangaMyAnimeListID == mangaId).FirstOrDefault();

                    // Get the Manga & check the type
                    if (dbManga is null)
                    {
                        await Task.Delay(350);
                        manga = await _jikan.GetMangaAsync(mangaId);
                        isMangaType = MangasHelper.IsMALMangaCorrectType(manga!.Data);

                        if (!isMangaType)
                            return;
                    }
                }
            }

            dbManga ??= await _mangasHelper.AddMALManga(manga!.Data, null);

            dbManga.IsSeasonManga = true;

            await _mangaRepo.Update(dbManga);

            await _cacheHelper.DeleteCache(CacheKey.MANGA_DTO, dbManga.Id.ToString());
            await _cacheHelper.DeleteCache(CacheKey.MANGA_TAGS);

            foreach (var tag in dbManga.Tags)
            {
                await _cacheHelper.DeleteCache(CacheKey.MANGA_TAGS, tag.Name.ToLower());
            }
            await Task.Delay(1000);
        }
        #endregion

        #region Metadata
        public async Task<int> UpdateAllMangasMetadata() 
        {
            int updatedMangas = 0;

            var mangas = await _mangaRepo.GetAll();
            foreach (var manga in mangas)
            {
                updatedMangas += (await UpdateMangaMetadata(manga)) ? 1 : 0;
            }

            return updatedMangas;
        }
        private async Task<bool> UpdateMangaMetadata(Core.Entities.Mangas.Manga manga)
        {
            bool changed = false;

            // Get MAL first
            if (manga.MangaMyAnimeListID != null)
            {
                var mangaMetadata = (await _jikan.GetMangaAsync(manga.MangaMyAnimeListID.Value)).Data;

                if (!MangasHelper.IsMALMangaCorrectType(mangaMetadata))
                {
                    Log.Information($"Deleting {manga.Name} because type is: {mangaMetadata.Type}");
                    await _mangaRepo.Delete(manga.Id);
                    return false;
                }

                //Update If needed
                if (mangaMetadata.Synopsis != manga.Description)
                {
                    manga.Description = mangaMetadata.Synopsis;
                    changed = true;
                }

                if (mangaMetadata.Published.To != null && manga.FinishedAt == null)
                {
                    manga.FinishedAt = DateTime.SpecifyKind(mangaMetadata.Published.To.Value, DateTimeKind.Utc);
                    manga.IsReleasing = false;
                    changed = true;
                }

                if (!manga.ImagesUrls.Contains(mangaMetadata.Images.JPG.ImageUrl))
                {
                    manga.ImagesUrls = new List<string>
                        {
                            mangaMetadata.Images.JPG.ImageUrl
                        };
                    changed = true;
                }

                if (manga.MALPopularity != mangaMetadata.Members)
                {
                    manga.MALPopularity = mangaMetadata.Members;
                    changed = true;
                }

                if (mangaMetadata.Score.HasValue)
                {
                    if (manga.MALScore != decimal.ToDouble(mangaMetadata.Score.Value))
                    {
                        manga.MALScore = decimal.ToDouble(mangaMetadata.Score.Value);
                        changed = true;
                    }
                }

                var isNSFW = MangasHelper.IsMangaNSFW(manga.Tags.Select(t => t.Name));
                if (isNSFW != manga.IsNSFW)
                {
                    manga.IsNSFW = isNSFW;
                    changed = true;
                }
            }

            // Get AL second
            if (manga.MangaAniListID != null)
            {
                var mangaMetadata = await AniListHelper.GetMangaById(manga.MangaAniListID.Value);

                if (mangaMetadata.Format.ToLowerInvariant() != "manga" && manga.MangaMyAnimeListID == null)
                {
                    Log.Information($"Deleting {manga.Name} because format is: {mangaMetadata.Format}");
                    await _mangaRepo.Delete(manga.Id);
                    return false;
                }

                //Only if MAL not found you update here
                if (!changed && manga.MangaMyAnimeListID == null)
                {

                    //Update If needed
                    if (mangaMetadata.Description != manga.Description)
                    {
                        manga.Description = mangaMetadata.Description;
                        changed = true;
                    }

                    if (mangaMetadata.EndDate.Year != null && manga.FinishedAt == null)
                    {
                        manga.FinishedAt = DateTime.SpecifyKind(new DateTime(mangaMetadata.EndDate.Year.Value, mangaMetadata.EndDate.Month ?? 1, mangaMetadata.EndDate.Day ?? 1), DateTimeKind.Utc);
                        manga.IsReleasing = false;
                        changed = true;
                    }

                    if (!manga.ImagesUrls.Contains(mangaMetadata.CoverImage.Large))
                    {
                        manga.ImagesUrls = new List<string>
                            {
                                mangaMetadata.CoverImage.Large
                            };
                        changed = true;
                    }
                }

                //Update title when is KR or CN
                if (manga.Type != MangaTypeEnum.MANGA && !string.IsNullOrEmpty(mangaMetadata.Title.English) && manga.Name != mangaMetadata.Title.English)
                {
                    manga.Name = mangaMetadata.Title.English;
                    changed = true;
                }

                //Get Tags if MAL doesn't have
                if (manga.Tags == null || !manga.Tags.Any())
                {
                    // Get Tags
                    var tags = new List<MangaTag>();
                    var mangaGenres = mangaMetadata.Genres;
                    foreach (var genre in mangaGenres)
                    {
                        var tag = await _mangaTagRepo.GetTagByName(genre);
                        tag ??= new MangaTag
                        {
                            Name = genre,
                        };

                        tags.Add(tag);
                    }

                    manga.Tags = tags;
                    changed = true;
                }

                var isNSFW = MangasHelper.IsMangaNSFW(manga.Tags.Select(t => t.Name));
                if (isNSFW != manga.IsNSFW)
                {
                    manga.IsNSFW = isNSFW;
                    changed = true;
                }

                if (manga.ALPopularity != mangaMetadata.Popularity)
                {
                    manga.ALPopularity = mangaMetadata.Popularity;
                    changed = true;
                }

                if (mangaMetadata.AverageScore.HasValue)
                {
                    if (manga.ALScore != mangaMetadata.AverageScore.Value / (double)10)
                    {
                        manga.ALScore = mangaMetadata.AverageScore.Value / (double)10;
                        changed = true;
                    }
                }
                else if (mangaMetadata.MeanScore.HasValue)
                {
                    if (manga.ALScore != mangaMetadata.MeanScore.Value / (double)10)
                    {
                        manga.ALScore = mangaMetadata.MeanScore.Value / (double)10;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                Log.Information($"Manga {manga.Name} just updated metadata");
                await _mangaRepo.Update(manga);
                await Task.Delay(1000);
                if (manga.MangaMyAnimeListID != null)
                    await Task.Delay(500);

                return true;
            }

            return false;
        }
        #endregion

        #region Download & Get Images
        public async Task DownloadImagesAndUpdateChapters()
        {
            var mangas = await _mangaRepo.GetAll();
            foreach (var manga in mangas)
            {
                Log.Information($"Updating Images for: {manga.Name}");

                var chaptersSavedIds = await CheckImagesSavedInFileServer(manga);
                await DownloadImagesFromMangaChapters(manga, chaptersSavedIds);

                await _mangaRepo.Update(manga);
                Log.Information($"Done Downloading Images for: {manga.Name}");
            }
        }
        private async Task<List<int>> CheckImagesSavedInFileServer(Core.Entities.Mangas.Manga manga)
        {
            var chaptersSavedIds = new List<int>();

            // Check Already Saved Images
            var mangaImagesStatus = await _imageService.GetAllMangaChapters(manga.Name.NormalizeStringToDirectory());
            if (mangaImagesStatus is null)
                return new List<int>();

            foreach (var chapterStatus in mangaImagesStatus.Chapters)
            {
                var chapter = manga.Chapters.SingleOrDefault(c => chapterStatus.Name.Replace("-", ".").Trim() == c.Name.ToString().Trim());
                if (chapter == null)
                {
                    if (!double.TryParse(chapterStatus.Name.Replace("-", "."), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var number))
                        continue;

                    var translationEnum = chapterStatus.Translation == "pt" ? TranslationLanguageEnum.PT : TranslationLanguageEnum.EN;

                    manga.Chapters.Add(new MangaChapter
                    {
                        Name = number,
                        Translations = new List<MangaTranslation>
                        {
                            new MangaTranslation
                            {
                                Language = translationEnum,
                                Downloaded = true,
                                IsWorking = true,
                                ReleasedDate = DateTime.UtcNow,
                            }
                        }
                    });
                }
                else
                {
                    // Add only translation
                    if (!chapter.Translations.Any(t => t.Language.ToString().ToLower() == chapterStatus.Translation))
                    {
                        chapter.Translations.Add(new MangaTranslation 
                        {
                            Downloaded = true,
                            IsWorking = true,
                            Language = chapterStatus.Translation == "pt" ? TranslationLanguageEnum.PT : TranslationLanguageEnum.EN,
                            ReleasedDate = DateTime.UtcNow,
                            MangaChapterId = chapter.Id
                        });
                    }

                    foreach (var translation in chapter.Translations)
                    {
                        translation.Downloaded = translation.Language.ToString().ToLower() == chapterStatus.Translation;
                        if (translation.Downloaded)
                            translation.IsWorking = true;
                    }
                    chaptersSavedIds.Add(chapter.Id);
                }
            }
            return chaptersSavedIds;
        }
        private async Task DownloadImagesFromMangaChapters(Core.Entities.Mangas.Manga manga, List<int> chaptersSavedIds)
        {
            // Download Images per sources
            foreach (var source in manga.Sources)
            {
                try
                {
                    // Save PT only
                    var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                    var scrapper = _scrappers.First(s => s.GetMangaSourceEnumDTO() == scrapperEnum);
                    if (scrapper.GetLanguage() != MangaTranslationEnumDTO.PT) continue;

                    var fetchedManga = await scrapper.GetManga(source.Url);
                    if (fetchedManga.Chapters == null || fetchedManga.Chapters.Count == 0) continue;
                    var chapters = manga.Chapters.Where(c => c.Translations.Any(t => t.Language == TranslationLanguageEnum.PT && !t.Downloaded));
                    foreach (var chapter in chapters)
                    {
                        try
                        {
                            var fetchedChapter = fetchedManga.Chapters
                                .First(c => c.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",", ".").Trim() ==
                                chapter.Name.ToString().Replace(",", "."));


                            var translation = chapter.Translations.First(t => t.Language == TranslationLanguageEnum.PT && !t.Downloaded);
                            if (await SaveImage(scrapper, TranslationLanguageEnum.PT, manga.Name, chapter.Name.ToString(), fetchedChapter.Link))
                            {
                                await Task.Delay(2000);
                            }

                            translation.Downloaded = true;

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

        }
        private async Task<bool> SaveImage(IBaseMangaScrapper scrapper, TranslationLanguageEnum scrapperTranslation, string mangaName, string chapterName, string chapterLink, bool replace = false)
        {
            try
            {
                var mangaNameSimplified = mangaName.NormalizeStringToDirectory();
                var folderPath = MangasHelper.BuildTranslationFolderPath(mangaNameSimplified, scrapperTranslation.ToString().ToLower(), chapterName.NormalizeStringToDirectory());
                if (Directory.Exists(folderPath) && !replace)
                    return false;


                var chapterPages = await scrapper.GetChapterImages(chapterLink);
                for (int i = 0; i < chapterPages.Count; i++)
                {
                    using WebClient webClient = new();
                    webClient.Headers.Add("user-agent", "User Agent");
                    var page = chapterPages[i];
                    webClient.Headers.Add("referer", $"{scrapper.GetBaseURLForManga()}/{chapterLink}");

                    var data = webClient.DownloadData(page);
                    await _imageService.SaveImage(data, $"{folderPath}/{i}.{page.Split(".").Last()}");
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new SavingImageException(ex.Message);
            }
        }
        #endregion

        #region New Chapters
        public async Task AddAllMangasToChaptersQueue()
        {
            var mangas = await _mangaRepo.GetAll();

            foreach (var manga in mangas)
            {
                if (manga.Sources.Any())
                    _queue.AddToQueue(new QueuedManga { MangaId = manga.Id, QueuedAt = DateTime.UtcNow });
            }

        }
        public async Task UpdateMangaChapters(QueuedManga queuedManga)
        {
            var manga = await _mangaRepo.GetById(queuedManga.MangaId);
            Log.Information($"Started Updating Chapters for {manga!.Name}");

            // Reset IsWorking Status
            foreach (var chapter in manga.Chapters)
            {
                foreach (var translation in chapter.Translations)
                {
                    if (!translation.Downloaded)
                        translation.IsWorking = false;
                }
            }

            await StartFetchingMangaChapters(manga, queuedManga); // QUEUE

            var addedChaptersNames = new List<double>();
            foreach (var source in manga.Sources)
            {
               addedChaptersNames.AddRange(await UpdateChaptersFromMangaAndSource(manga, source, manga.Sources.Count));
            }
            addedChaptersNames = addedChaptersNames.Distinct().Order().ToList();

            if (addedChaptersNames.Any())
            {
                var usersToNotify = (await _mangaUserDataRepo.GetUsersByStatusByManga(manga.Id, MangaUserStatusEnum.READING)).Where(s => s.User.ServerMember != null).Select(s => s.User.ServerMember!.DiscordId).ToList();
                
                if (usersToNotify.Any())
                    await _mangasHub.Clients.All.ChaptersAdded(usersToNotify, manga.Name, addedChaptersNames);
            }


            // With this here only adds all the chapters at the same time
            await _mangaRepo.Update(manga);

            await _cacheHelper.DeleteCache(CacheKey.MANGA_DTO, manga.Id.ToString());
            await _cacheHelper.DeleteCache(CacheKey.RECENT_CHAPTERS);

            var chaptersFetchingQueue = await FinishCurrentQueuedManga(); // QUEUE

            // Notify it ended
            await _mangasHub.Clients.All.SendMangasQueue(chaptersFetchingQueue);
        }

        // Starting Manga
        private async Task<List<QueuedMangaDTO>> StartFetchingMangaChapters(Core.Entities.Mangas.Manga manga, QueuedManga queuedManga)
        {
            var dtosQueue = new List<QueuedMangaDTO>();
            var user = queuedManga.QueuedByUser != null ? await _userRepository.GetById(queuedManga.QueuedByUser.Value) : null;
            dtosQueue.Add(new QueuedMangaDTO
            {
                Manga = _mapper.Map<MangaInfoDTO>(manga),
                Progress = new QueueProgressDTO(),
                QueuedAt = queuedManga.QueuedAt,
                QueuedByUser = user != null ? _mapper.Map<SimpleUserDTO>(user) : null
            });
            var cachedQueue = await RefreshMangasQueuePositions(dtosQueue); 
            await _cacheHelper.SetCache(CacheKey.CHAPTERS_QUEUE, cachedQueue);
            return cachedQueue;
        }

        // Updating Manga / Progress
        private async Task<List<QueuedMangaDTO>> UpdateMangasQueueProgress(int i, int chaptersCount, int maxSources, MangaSourceEnumDTO scrapper)
        {
            var cachedQueue = await _cacheHelper.GetCache<List<QueuedMangaDTO>>(CacheKey.CHAPTERS_QUEUE);
            if (cachedQueue is not null)
            {
                cachedQueue![0].Progress!.TotalChapters = chaptersCount;
                cachedQueue![0].Progress!.CurrentSource = scrapper;
                cachedQueue![0].Progress!.CurrentChapter = i + 1;
                cachedQueue![0].Progress!.Percentage += ((100 / (double)maxSources) / chaptersCount);

                cachedQueue = await RefreshMangasQueuePositions(cachedQueue);
                await _cacheHelper.SetCache(CacheKey.CHAPTERS_QUEUE, cachedQueue);
                return cachedQueue;
            }
            return cachedQueue ?? new List<QueuedMangaDTO>();
        }

        // Finished Manga
        private async Task<List<QueuedMangaDTO>> FinishCurrentQueuedManga()
        {
            var cachedQueue = await _cacheHelper.GetCache<List<QueuedMangaDTO>>(CacheKey.CHAPTERS_QUEUE); 
            if (cachedQueue != null)
            {
                cachedQueue.RemoveAt(0);
                cachedQueue = await RefreshMangasQueuePositions(cachedQueue);
                await _cacheHelper.SetCache(CacheKey.CHAPTERS_QUEUE, cachedQueue);
            }
            return cachedQueue ?? new List<QueuedMangaDTO>();
        }
        
        private async Task<List<QueuedMangaDTO>> RefreshMangasQueuePositions(List<QueuedMangaDTO> previousQueue)
        {
            var updatedQueue = _queue.GetNextInQueue();

            // Get New Ones
            var newQueue = new List<QueuedMangaDTO>();
            if (previousQueue!.Any())
                newQueue.Add(previousQueue[0]);

            foreach (var manga in updatedQueue)
            {
                // Check if the next manga was already cached on the queue
                var queuedManga = previousQueue.FirstOrDefault(cm => cm.Manga.Id == manga.MangaId && cm.QueuedByUser?.Id == manga.QueuedByUser && cm.QueuedAt == manga.QueuedAt);
                if (queuedManga is null)
                {
                    var repeatedManga = previousQueue.Select(cm => cm.Manga).FirstOrDefault(cm => cm.Id == manga.MangaId);
                    repeatedManga ??= _mapper.Map<MangaInfoDTO>(await _mangaRepo.GetById(manga.MangaId));

                    var repeatedUser = manga.QueuedByUser is null ? null : previousQueue.Select(cm => cm.QueuedByUser).FirstOrDefault(cm => cm?.Id == manga.QueuedByUser);
                    repeatedUser ??= manga.QueuedByUser is null ? null : _mapper.Map<SimpleUserDTO>(await _userRepository.GetById(manga.QueuedByUser.Value));

                    queuedManga = new QueuedMangaDTO
                    {
                        QueuedAt = manga.QueuedAt,
                        QueuedByUser = repeatedUser,
                        Manga = repeatedManga
                    };
                }
                newQueue.Add(queuedManga!);
            }

            return newQueue;

        }

        private async Task<List<double>> UpdateChaptersFromMangaAndSource(Core.Entities.Mangas.Manga manga, MangaSource source, int maxSources)
        {
            List<double> chaptersAdded = new();
            try
            {
                Log.Information($"Source: {source.Source}");

                // Get Scrapper
                var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                var scrapper = _mangasHelper.GetScrapperByEnum(scrapperEnum);
                if (scrapper == null)
                    throw new Exception("No Scrapper Registered");
                var scrapperTranslation = _mapper.Map<TranslationLanguageEnum>(scrapper?.GetLanguage());

                // Get (Cached) Manga Info
                var mangaInfo = await _cacheHelper.GetCache<ScrappedManga?>(CacheKey.CUSTOM, source.Url);
                if (mangaInfo == null)
                {
                    mangaInfo = await scrapper!.GetManga(source.Url);
                    if (mangaInfo == null) return new List<double>();

                    await _cacheHelper.SetCache(CacheKey.CUSTOM, mangaInfo, source.Url);
                }

                int scrapperFailures = 0;

                for (int i = 0; i < mangaInfo?.Chapters.Count; i++)
                {
                    var chapter = mangaInfo.Chapters[i];

                    // Send to Queue Hub
                    await _mangasHub.Clients.All.SendMangasQueue(await UpdateMangasQueueProgress(i, mangaInfo.Chapters.Count, maxSources, scrapperEnum));

                    // Check if its a valid name
                    var nameScrapped = chapter.Name.Split(":").First().Split("-").First().Split(" ").First().Replace(",", ".").Trim();
                    if (string.IsNullOrEmpty(nameScrapped) || !double.TryParse(nameScrapped, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var number))
                        continue;

                    // Get Chapter Translation from db if exists (change all Translations to not work)
                    var dbChapter = manga.Chapters.FirstOrDefault(c => c.Name.ToString() == number.ToString());
                    dbChapter ??= new MangaChapter
                    {
                        Name = number,
                    };
                    dbChapter.Translations ??= new();
                    var dbTranslation = dbChapter.Translations.FirstOrDefault(t => t.Language == scrapperTranslation);

                    // If doesn't exist create
                    if (dbTranslation is null)
                    {
                        try
                        {
                            var translation = new MangaTranslation
                            {
                                ReleasedDate = chapter.ReleasedAt is not null ? DateTime.SpecifyKind(chapter.ReleasedAt.Value, DateTimeKind.Utc) : null,
                                Language = scrapperTranslation,
                                IsWorking = true,
                                Downloaded = false
                            };

                            dbChapter.Translations.Add(translation);
                            manga.Chapters.Add(dbChapter);
                            scrapperFailures = 0;

                            chaptersAdded.Add(number);

                            Log.Information($"Added Chapter: {dbChapter.Name} for {scrapperTranslation}");

                        }
                        catch (SavingImageException sie)
                        {
                            Log.Error(sie, $"Chapter {nameScrapped} failed saving image");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Chapter {nameScrapped} failed saving");

                            var sourceFailedTooManyTimesInARow = ++scrapperFailures == 5;
                            if (sourceFailedTooManyTimesInARow)
                            {
                                Log.Error($"Something might be wrong for the {source.Source} and {source.Name}");
                                break;
                            }
                        }
                        finally
                        {
                            await Task.Delay(1500);
                        }
                    }
                    else if (dbTranslation.IsWorking || dbTranslation.Downloaded)
                    {
                        //Avoid rewriting a chapter already working
                        continue;
                    }
                    // If exists update to work
                    else
                    {
                        dbTranslation.IsWorking = true;
                        if (chapter.ReleasedAt.HasValue && (dbTranslation.ReleasedDate is null || DateTime.SpecifyKind(chapter.ReleasedAt.Value, DateTimeKind.Utc) < dbTranslation.ReleasedDate))
                            dbTranslation.ReleasedDate = DateTime.SpecifyKind(chapter.ReleasedAt.Value, DateTimeKind.Utc);

                        Log.Debug($"Chapter {nameScrapped} already in DB");
                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error Updating {manga.Name}: {ex.Message}\n{ex.StackTrace}");
            }

            return chaptersAdded;
        }

        #endregion

    }
}
