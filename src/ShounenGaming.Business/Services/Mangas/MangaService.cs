using AutoMapper;
using JikanDotNet;
using Serilog;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Business.Services.Mangas_Scrappers;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System.Globalization;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaService : IMangaService
    {
        private static readonly double recommendations_type_growth = 1.5;
        private static readonly double recommendations_tags_growth = 1;

        private readonly IUserRepository _userRepository;
        private readonly IMangaRepository _mangaRepo;
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IMangaWriterRepository _mangaWriterRepo;
        private readonly IMangaTagRepository _mangaTagRepo;

        private readonly IJikan _jikan;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        private readonly IFetchMangasQueue _queue;
        private readonly CacheHelper _cacheHelper;
        private readonly MangasHelper _mangasHelper;

        private readonly IEnumerable<IBaseMangaScrapper> _scrappers;

        public MangaService(IMangaRepository mangaRepo, IMangaWriterRepository mangaWriterRepo, IMangaTagRepository mangaTagRepo, IMapper mapper, IImageService imageService, IJikan jikan, IUserRepository userRepository, IFetchMangasQueue queue, IEnumerable<IBaseMangaScrapper> scrappers, IMangaUserDataRepository mangaUserDataRepo, CacheHelper cacheHelper, MangasHelper mangasHelper)
        {
            _mangaRepo = mangaRepo;
            _mangaWriterRepo = mangaWriterRepo;
            _mangaTagRepo = mangaTagRepo;
            _mapper = mapper;
            _imageService = imageService;
            _jikan = jikan;
            _userRepository = userRepository;
            _queue = queue;
            _scrappers = scrappers;
            _mangaUserDataRepo = mangaUserDataRepo;
            _cacheHelper = cacheHelper;
            _mangasHelper = mangasHelper;
        }


        public async Task<MangaDTO> GetMangaById(int id)
        {
            return (await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.MANGA_DTO, async _ =>
            {
                var manga = await _mangaRepo.GetById(id);
                if (manga is null)
                    throw new EntityNotFoundException("Manga");

                return _mapper.Map<MangaDTO>(manga);
            }, id.ToString()))!;
        }
        public async Task<List<MangaSourceDTO>> GetMangaSourcesById(int id)
        {
            return await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.MANGA_SOURCES_DTO, async _ =>
            {
                var manga= await _mangaRepo.GetById(id);
                if (manga is null) 
                    throw new EntityNotFoundException("Manga");
                return _mapper.Map<List<MangaSourceDTO>>(manga.Sources);
            }, id.ToString()) ?? new List<MangaSourceDTO>();
        }
        public async Task<MangaTranslationDTO?> GetMangaTranslation(int userId, int mangaId, int chapterId, MangaTranslationEnumDTO translation)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new EntityNotFoundException("User");

            var manga = await _mangaRepo.GetById(mangaId);
            if (manga is null) 
                throw new EntityNotFoundException("Manga");

            var mangaChapter = manga.Chapters.FirstOrDefault(c => c.Id == chapterId) ?? 
                throw new EntityNotFoundException("MangaChapter");

            var mangaTranslation = mangaChapter.Translations.FirstOrDefault(t => t.Language == _mapper.Map<TranslationLanguageEnum>(translation) && t.IsWorking);
            mangaTranslation ??= mangaChapter.Translations.FirstOrDefault(t => t.IsWorking) ?? throw new EntityNotFoundException("MangaTranslation");

            var selectedSource = string.Empty;

            // ----- Fetch Images -----
            // Saved in File Server
            if (mangaTranslation.Downloaded)
            {
                var mangaNameSimplified = manga.Name.NormalizeStringToDirectory();
                var files = await _imageService.GetChapterImages(mangaNameSimplified, 
                    mangaChapter.Name.ToString().NormalizeStringToDirectory(), 
                    mangaTranslation.Language.ToString().ToLower());
                
                return _mangasHelper.MapMangaTranslation(mangaTranslation, selectedSource, files, user.MangasConfigurations.SkipChapterToAnotherTranslation);
            }

            // Not Saved (Fetch from Websites)
            var pages = new List<string>();
            foreach (var source in manga.Sources)
            {
                try
                {
                    var scrapperEnum = (MangaSourceEnumDTO)Enum.Parse(typeof(MangaSourceEnumDTO), source.Source);
                    var scrapper = _mangasHelper.GetScrapperByEnum(scrapperEnum);
                    if (scrapper == null)
                        continue;

                    var scrapperTranslation = _mapper.Map<TranslationLanguageEnum>(scrapper?.GetLanguage());
                    if (scrapperTranslation != mangaTranslation.Language)
                        continue;

                    var mangaInfo = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.SCRAPPED_MANGA, async _ =>
                    {
                        return await scrapper!.GetManga(source.Url);
                    }, mangaId.ToString());

                    foreach (var c in mangaInfo!.Chapters)
                    {
                        var treatedName = c.Name.Split(":").First().Split("-").First().Split(" ").First().Trim();
                        if (string.IsNullOrEmpty(treatedName) || !double.TryParse(treatedName, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var number))
                            continue;

                        if (number == mangaTranslation.MangaChapter.Name)
                        {
                            selectedSource = source.Source; 
                            pages = await scrapper!.GetChapterImages(c.Link);

                            return _mangasHelper.MapMangaTranslation(mangaTranslation, selectedSource, pages, user.MangasConfigurations.SkipChapterToAnotherTranslation);
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error Fetching Manga Translation");
                }
            }

            return null;
        }
        public async Task<PaginatedResponse<MangaInfoDTO>> SearchMangas(SearchMangaQueryDTO query, int page, int? userId = null)
        {
            var includeNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value);
                if (user is null)
                    throw new EntityNotFoundException("User");
                includeNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var mangas = await _mangaRepo.SearchManga(page, includeNSFW, query.Name, userId);

            if (!showProgressAll)
                mangas.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return new PaginatedResponse<MangaInfoDTO>
            {
                CurrentPage = page,
                Data = _mapper.Map<List<MangaInfoDTO>>(mangas),
                MaxCount = await _mangaRepo.GetAllCount(includeNSFW, query.Name, userId)
            };
        }
        public async Task<List<MangaInfoDTO>> GetWaitingMangas()
        {
            var waitingMangas = await _mangaRepo.GetWaitingMangas();
            return _mapper.Map<List<MangaInfoDTO>>(waitingMangas);
        }

        public async Task<List<MangaInfoDTO>> GetSeasonMangas()
        {
            return await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.SEASON_MANGAS, async _ =>
            {
                var mangas = await _mangaRepo.GetSeasonMangas();
                return _mapper.Map<List<MangaInfoDTO>>(mangas);
            }) ?? new List<MangaInfoDTO>();
        }

        public async Task<List<MangaInfoDTO>> GetPopularMangas(int? userId = null)
        {
            var includesNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value) ?? throw new EntityNotFoundException("User");
                includesNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var popularMangas = await _mangaRepo.GetPopularMangas(includesNSFW);

            if (!showProgressAll)
                popularMangas.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return _mapper.Map<List<MangaInfoDTO>>(popularMangas);
        }
       
        public async Task<List<MangaInfoDTO>> GetRecentlyAddedMangas()
        {
            return await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.RECENT_MANGAS, async _ =>
            {
                var mangas = await _mangaRepo.GetRecentlyAddedMangas();
                return _mapper.Map<List<MangaInfoDTO>>(mangas.Take(15));
            }) ?? new List<MangaInfoDTO>();
        }
        public async Task<List<LatestReleaseMangaDTO>> GetRecentlyReleasedChapters(int? userId = null)
        {
            var includeNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value);
                if (user is null)
                    throw new EntityNotFoundException("User");
                includeNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var result = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.RECENT_CHAPTERS, async _ => 
            {
                var mangas = await _mangaRepo.GetRecentlyReleasedChapters(includeNSFW);
                var dto = new List<LatestReleaseMangaDTO>();
                foreach (var manga in mangas)
                {
                    var dic = new Dictionary<MangaTranslationEnumDTO, ChapterReleaseDTO>();
                    var orderedList = manga.Chapters.OrderByDescending(c => c.Name).ToList();
                    foreach (var chapter in orderedList)
                    {
                        foreach (var translation in chapter.Translations.Where(t => t.IsWorking))
                        {
                            var language = _mapper.Map<MangaTranslationEnumDTO>(translation.Language);
                            if (!dic.ContainsKey(language))
                            {
                                dic.Add(language, new ChapterReleaseDTO
                                {
                                    Id = chapter.Id,
                                    Name = chapter.Name,
                                    Translation = language,
                                    CreatedAt = translation.CreatedAt
                                });
                            }
                        }

                        if (dic.Count == Enum.GetNames(typeof(MangaTranslationEnumDTO)).Length) break;
                    }

                    if (!showProgressAll)
                        manga.Chapters = manga.Chapters.Where(c => (c.Name % 1) == 0).ToList();

                    dto.Add(new LatestReleaseMangaDTO
                    {
                        Manga = _mapper.Map<MangaInfoDTO>(manga),
                        ReleasedChapters = dic
                    });
                }
                return dto;

            }, includeNSFW.ToString());

            return result!;
        }
        public async Task<List<MangaWriterDTO>> GetMangaWriters()
        {
            var writers = await _mangaWriterRepo.GetAll();
            return _mapper.Map<List<MangaWriterDTO>>(writers);
        }
        
        public async Task<List<MangaInfoDTO>> GetMangasFromTag(string tag, int? userId = null)
        {
            var includesNSFW = true;
            var showProgressAll = true;

            if (userId != null)
            {
                var user = await _userRepository.GetById(userId.Value);
                if (user is null)
                    throw new EntityNotFoundException("User");
                includesNSFW = user.MangasConfigurations.NSFWBehaviour != NSFWBehaviourEnum.HIDE_ALL;
                showProgressAll = user.MangasConfigurations.ShowProgressForChaptersWithDecimals;
            }

            var mangas = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.MANGA_TAGS, async _ => await _mangaRepo.GetMangasByTag(tag , includesNSFW), $"{tag}_{includesNSFW}");

            if (!showProgressAll)
                mangas?.ForEach(m => m.Chapters = m.Chapters.Where(c => (c.Name % 1) == 0).ToList());

            return _mapper.Map<List<MangaInfoDTO>>(mangas);
        }
        public async Task<List<string>> GetMangaTags()
        {
            var tags = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.MANGA_TAGS, async _ => await _mangaTagRepo.GetAll()) ?? new List<MangaTag>();
            return tags.Select(t => t.Name).ToList();
        }
        public async Task<MangaWriterDTO> GetMangaWriterById(int id)
        {
            var writer = await _mangaWriterRepo.GetById(id);
            return _mapper.Map<MangaWriterDTO>(writer);
        }

        public async Task<List<MangaMetadataDTO>> SearchMangaMetadata(MangaMetadataSourceEnumDTO source, string name)
        {
            switch (source)
            {
                case MangaMetadataSourceEnumDTO.ANILIST:
                    var aniMangas = await AniListHelper.SearchMangaByName(name);
                    var alDtos = new List<MangaMetadataDTO>();
                    foreach(var m in aniMangas)
                    {
                        alDtos.Add(await ConvertALMangaToDTO(m));
                    }
                    return alDtos;

                case MangaMetadataSourceEnumDTO.MYANIMELIST:
                    var malMangas = (await _jikan.SearchMangaAsync(name)).Data;

                    // Filter only allowed types
                    malMangas = MangasHelper.FilterCorrectTypes(malMangas);

                    var malDtos = new List<MangaMetadataDTO>();
                    foreach(var m in malMangas)
                    {
                        malDtos.Add(await ConvertMALMangaToDTO(m));
                    }
                    return malDtos;
                   
                default:
                    return new List<MangaMetadataDTO>();
            }
        }
        private async Task<MangaMetadataDTO> ConvertALMangaToDTO(AniListHelper.ALManga m, bool mayExist = true)
        {
            var titles = new List<string>();
            if (m.Title.UserPreferred != null)
                titles.Add(m.Title.UserPreferred);

            if (m.Title.English != null)
                titles.Add(m.Title.English);

            if (m.Title.Native != null)
                titles.Add(m.Title.Native);

            if (m.Title.Romaji != null)
                titles.Add(m.Title.Romaji);

            return new MangaMetadataDTO
            {
                Id = m.Id,
                Titles = titles,
                AlreadyExists = mayExist && await _mangaRepo.MangaExistsByAL(m.Id),
                Description = m.Description ?? "",
                Tags = m.Genres.ToList(),
                Status = m.Status,
                Score = (m.AverageScore is not null ? m.AverageScore / (decimal)10 : m.MeanScore / (decimal)10) ?? 0,
                FinishedAt = m.EndDate.Year != null ? new DateTime(m.EndDate.Year.Value, m.EndDate.Month ?? 1, m.EndDate.Day ?? 1) : null,
                ImageUrl = m.CoverImage.Large,
                StartedAt = m.StartDate.Year != null ? new DateTime(m.StartDate.Year.Value, m.StartDate.Month ?? 1, m.StartDate.Day ?? 1) : null,
                Type = _mapper.Map<MangaTypeEnumDTO>(MangasHelper.ConvertMALMangaType(m.CountryOfOrigin)),
                Source = MangaMetadataSourceEnumDTO.ANILIST
            };
        }
        private async Task<MangaMetadataDTO> ConvertMALMangaToDTO(JikanDotNet.Manga m, bool mayExist = true)
        {
            return new MangaMetadataDTO
            {
                Id = m.MalId,
                Titles = m.Titles.Select(t => t.Title).ToList(),
                Description = m.Synopsis ?? "",
                FinishedAt = m.Published.To,
                AlreadyExists = mayExist && await _mangaRepo.MangaExistsByMAL(m.MalId),
                Type = m.Type == "manhwa" ? MangaTypeEnumDTO.MANWHA : (m.Type == "manhua" ? MangaTypeEnumDTO.MANHUA : MangaTypeEnumDTO.MANGA),
                Status = m.Status,
                StartedAt = m.Published.From,
                ImageUrl = m.Images.JPG.ImageUrl,
                Score = m.Score ?? 0,
                Tags = m.Genres.Select(g => g.Name).ToList(),
                Source = MangaMetadataSourceEnumDTO.MYANIMELIST
            };
        }
        
        public async Task<List<MangaInfoDTO>> GetMangaRecommendations(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new EntityNotFoundException("User");

            var userData = await _mangaUserDataRepo.GetByUser(userId);
            if (userData is null)
                throw new EntityNotFoundException("UserData");

            // If no readings yet -> Popular
            if (!userData.Any())
            {
                return _mapper.Map<List<MangaInfoDTO>>(await _mangaRepo.GetPopularMangas());
            }

            // If readings search the most common tags
            var tagsScores = new Dictionary<string, double>();
            var typesScores = new Dictionary<MangaTypeEnum, double>();
            foreach (var manga in userData)
            {
                if (manga.Status == MangaUserStatusEnum.DROPPED || manga.Status == MangaUserStatusEnum.IGNORED) continue;

                //Types
                if (typesScores.ContainsKey(manga.Manga.Type))
                {
                    typesScores[manga.Manga.Type] += (manga.Rating ?? 2.5) * recommendations_type_growth * (manga.Manga.IsNSFW ? 0.5 : 1);
                }
                else typesScores[manga.Manga.Type] = (manga.Rating ?? 2.5) * recommendations_type_growth * (manga.Manga.IsNSFW ? 0.5 : 1);

                //Tags
                foreach (var tag in manga.Manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag.Name))
                    {
                        tagsScores[tag.Name] += (manga.Rating ?? 2.5) * recommendations_tags_growth * (manga.Manga.IsNSFW ? 0.5 : 1);
                    }
                    else tagsScores[tag.Name] = (manga.Rating ?? 2.5) * recommendations_tags_growth * (manga.Manga.IsNSFW ? 0.5 : 1);
                }
            }

            // Search the mangas with OK rating which have most of those tags (not read before) and not on ignore !
            var allMangas = await _mangaRepo.GetAll();
            var allMangasNotSeen = allMangas.Where(m => !userData.Where(m => m.Status != MangaUserStatusEnum.PLANNED).Any(usm => usm.Manga.Id == m.Id)).OrderByDescending(a => ((a.ALScore ?? a.MALScore) + (a.MALScore ?? a.ALScore)) / 2);

            var mangasScores = new Dictionary<int, double>();
            foreach (var manga in allMangasNotSeen)
            {
                mangasScores[manga.Id] = 0;
                foreach (var tag in manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag.Name))
                        mangasScores[manga.Id] += tagsScores[tag.Name];
                }

                if (typesScores.ContainsKey(manga.Type))
                    mangasScores[manga.Id] += typesScores[manga.Type];
            }

            // TODO: Maybe put a "where value" bigger than something
            // TODO: Take points from Ignored

            var mangaIds = mangasScores.Where(ms => ms.Value > 0).OrderByDescending(ms => ms.Value).Select(ms => ms.Key).Take(25).ToList();
            return _mapper.Map<List<MangaInfoDTO>>(allMangasNotSeen.Where(m => mangaIds.Contains(m.Id)));
        }
        public async Task<List<MangaMetadataDTO>> SearchMangaRecommendations(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                throw new EntityNotFoundException("User");

            var userData = await _mangaUserDataRepo.GetByUser(userId);
            if (userData is null)
                throw new EntityNotFoundException("UserData");

            var userSeenMangas = userData.Where(m => m.Status == MangaUserStatusEnum.COMPLETED ||
                m.Status == MangaUserStatusEnum.READING ||
                m.Status == MangaUserStatusEnum.ON_HOLD);

            var allDTOs = new List<MangaMetadataDTO>();

            // If no readings yet -> Popular
            if (!userData.Any())
            {
                var malMangas = (await _jikan.GetTopMangaAsync()).Data;
                malMangas = MangasHelper.FilterCorrectTypes(malMangas);

                foreach (var m in malMangas)
                {
                    allDTOs.Add(await ConvertMALMangaToDTO(m));
                }

                var alMangas = await AniListHelper.GetPopularMangas();
                foreach (var m in alMangas)
                {
                    if (m.IdMal is not null && allDTOs.Any(a => a.Id == m.IdMal && a.Source == MangaMetadataSourceEnumDTO.MYANIMELIST))
                        continue;

                    allDTOs.Add(await ConvertALMangaToDTO(m));
                }
                return allDTOs.OrderByDescending(m => m.Score).ToList();
            }

            // Get most used Tags & Types
            var tagsScores = new Dictionary<string, double>();
            var typesScores = new Dictionary<MangaTypeEnumDTO, double>();
            foreach (var manga in userSeenMangas)
            {
                //Types
                var dtoType = _mapper.Map<MangaTypeEnumDTO>(manga.Manga.Type);
                if (typesScores.ContainsKey(dtoType))
                {
                    typesScores[dtoType] += (manga.Rating ?? 2.5) * recommendations_type_growth * (manga.Manga.IsNSFW ? 0.5 : 1);
                }
                else typesScores[dtoType] = (manga.Rating ?? 2.5) * recommendations_type_growth * (manga.Manga.IsNSFW ? 0.5 : 1);

                //Tags
                foreach (var tag in manga.Manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag.Name))
                    {
                        tagsScores[tag.Name] += (manga.Rating ?? 2.5) * recommendations_tags_growth * (manga.Manga.IsNSFW ? 0.5 : 1);
                    }
                    else tagsScores[tag.Name] = (manga.Rating ?? 2.5) * recommendations_tags_growth * (manga.Manga.IsNSFW ? 0.5 : 1);
                }
            }

            var allMangas = await _mangaRepo.GetAll();

            // Fetch Mangas
            var mostSeenTags = tagsScores.OrderByDescending(ms => ms.Value).Select(ms => ms.Key).Take(5).ToList();
            
            // Take min.20 from MAL
            int malI = 1;
            while (allDTOs.Count < 20 || malI == 5)
            {
                var searchedMALMangas = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.SEARCHED_MAL, async _ => (await _jikan.SearchMangaAsync(new MangaSearchConfig { Page = malI, OrderBy = MangaSearchOrderBy.Popularity })).Data, $"{malI}");
                searchedMALMangas = MangasHelper.FilterCorrectTypes(searchedMALMangas ?? new List<JikanDotNet.Manga>());
                var searchedMALMangasNotAdded = searchedMALMangas?.Where(sm => !allMangas.Any(am => am.MangaMyAnimeListID == sm.MalId) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.MYANIMELIST && dtos.Id == sm.MalId)).ToList();
                searchedMALMangasNotAdded?.ForEach(async s => allDTOs.Add(await ConvertMALMangaToDTO(s, false)));
                malI++;
                await Task.Delay(1000);
            }

            int page = 1;
            do 
            {
                //TODO: Take more Types and Search only for Types
                // Closer to Favorites
                var top3Tags = mostSeenTags.Take(3).ToList();
                var topType = typesScores.OrderByDescending(ts => ts.Value).Select(ts => ts.Key).FirstOrDefault();
                var searchedTopALMangas = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.SEARCHED_AL, async _ =>
                {
                    if (topType == MangaTypeEnumDTO.MANHUA)
                        return await AniListHelper.SearchManhuaByTags(top3Tags, page);
                    else if (topType == MangaTypeEnumDTO.MANWHA)
                        return await AniListHelper.SearchManwhaByTags(top3Tags, page);
                    else if (topType == MangaTypeEnumDTO.MANGA)
                        return await AniListHelper.SearchMangaByTags(top3Tags, page);

                    return await AniListHelper.SearchAllMangaTypeByTags(top3Tags, page);
                }, $"{top3Tags}_{topType}_{page}");
                var searchedTopALMangasNotAdded = searchedTopALMangas?.Where(sm =>
                    !allMangas.Any(am => am.MangaAniListID == sm.Id) && 
                    (sm.IdMal == null || !allMangas.Any(am => am.MangaMyAnimeListID == sm.IdMal))
                    && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.ANILIST && dtos.Id == sm.Id)
                    && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.MYANIMELIST && dtos.Id == sm.IdMal)).ToList();
                searchedTopALMangasNotAdded?.ForEach(async s => allDTOs.Add(await ConvertALMangaToDTO(s, false)));

                // From Single Tag
                foreach (var tag in mostSeenTags)
                {
                    var searchedALMangas = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.SEARCHED_AL, async _ => await AniListHelper.SearchAllMangaTypeByTags(new List<string>() { tag }, page), $"{tag}_{page}");
                    var searchedALMangasNotAdded = searchedALMangas?.Where(sm => !allMangas.Any(am => am.MangaAniListID == sm.Id) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.ANILIST && dtos.Id == sm.Id) && !allDTOs.Any(dtos => dtos.Source == MangaMetadataSourceEnumDTO.MYANIMELIST && dtos.Id == sm.IdMal)).ToList();
                    searchedALMangasNotAdded?.ForEach(async s => allDTOs.Add(await ConvertALMangaToDTO(s, false)));
                }


                page++;
            }
            while (allDTOs.Count < 100);
            

            // Calculate Mangas
            var mangasScores = new Dictionary<int, double>();
            for (int i = 0; i < allDTOs.Count; i++)
            {
                var manga = allDTOs[i];
                mangasScores[i] = 0;
                foreach (var tag in manga.Tags)
                {
                    if (tagsScores.ContainsKey(tag))
                        mangasScores[i] += tagsScores[tag];
                }

                if (typesScores.ContainsKey(manga.Type))
                    mangasScores[i] += typesScores[manga.Type];
            }

            var orderedDTOs = mangasScores.Where(ms => ms.Value > 0).OrderByDescending(ms => ms.Value).Select(ms => allDTOs[ms.Key]).ToList();
            return orderedDTOs;
        }



        public async Task<List<MangaSourceDTO>> SearchMangaSource(string name)
        {
            var treatedName = name.Replace("\"", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("!", "")
                .Replace("#", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("'", " ");

            var finalResults = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.MANGA_SOURCES_DTO, async _ =>  {
                List<Task<List<MangaSourceDTO>>> allMangasTasks = new();

                foreach (var scrapper in _scrappers)
                {
                    allMangasTasks.Add(scrapper.SearchManga(treatedName));
                }
                var result = await Task.WhenAll(allMangasTasks.ToArray());

                var listResults = new List<MangaSourceDTO>();
                foreach (var item in result)
                {
                    listResults.AddRange(item);
                }
                return listResults;
            }, treatedName);
           
            return finalResults ?? new List<MangaSourceDTO>();
        }
        public async Task<List<MangaSourceDTO>> LinkSourcesToManga(int mangaId, List<MangaSourceDTO> mangas)
        {
            var manga = await _mangaRepo.ClearSources(mangaId);
            if (manga is null)
                throw new EntityNotFoundException("Manga");

            manga.Sources = mangas.Select(m => new MangaSource { Source = m.Source.ToString(), Url = m.Url , ImageURL = m.ImageURL, Name = m.Name}).ToList();

            var dbManga = await _mangaRepo.Update(manga);

            await _cacheHelper.DeleteCache(CacheHelper.CacheKey.MANGA_SOURCES_DTO, mangaId.ToString());

            var result = await _cacheHelper.GetOrSetCache(CacheHelper.CacheKey.MANGA_SOURCES_DTO, async _ => await Task.FromResult(_mapper.Map<List<MangaSourceDTO>>(dbManga.Sources)), mangaId.ToString());
            return result!;
        }


        public async Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, int userId)
        {
            var user = await _userRepository.GetById(userId) ?? throw new EntityNotFoundException("User");

            //TODO: Handle Exceptions (Timeout and wrong id)
            var manga = source == MangaMetadataSourceEnumDTO.MYANIMELIST ? await _mangasHelper.AddMALManga(mangaId, user.Id) : await _mangasHelper.AddALManga(mangaId, user.Id);
            if (manga is null)
                throw new Exception("Couldn't create or get the Manga");

            await _cacheHelper.DeleteCache(CacheHelper.CacheKey.RECENT_MANGAS);
            return _mapper.Map<MangaDTO>(manga);
        }
        public async Task<MangaDTO> AddManga(MangaMetadataSourceEnumDTO source, long mangaId, string discordId)
        {
            var user = await _userRepository.GetUserByDiscordId(discordId) ?? throw new EntityNotFoundException("User");
            return await AddManga(source, mangaId, user.Id);
        }
      
        public async Task StartMangaChaptersUpdate(int mangaId, int userId)
        {
            var user = await _userRepository.GetById(userId) ?? throw new EntityNotFoundException("User");
            var manga = await _mangaRepo.GetById(mangaId) ?? throw new EntityNotFoundException("Manga");

            _queue.AddToPriorityQueue(new QueuedManga { MangaId = manga.Id, QueuedAt = DateTime.UtcNow, QueuedByUser = userId});
        }

        public async Task<List<QueuedMangaDTO>> GetQueueStatus()
        {
            var queue = await _cacheHelper.GetCache<List<QueuedMangaDTO>>(CacheHelper.CacheKey.CHAPTERS_QUEUE);
            return queue ?? new List<QueuedMangaDTO>();
        }

        public async Task FixDuplicatedChapters()
        {
            var mangas = await _mangaRepo.GetAll();

            foreach(var manga in mangas)
            {
                var duplicatedChapterNumbers = new List<double>();
                foreach(var chapter in manga.Chapters)
                {
                    var chaptersCount = manga.Chapters.Count(c => c.Name.ToString() == chapter.Name.ToString());
                    if (chaptersCount > 1)
                        duplicatedChapterNumbers.Add(chapter.Name);
                }
                duplicatedChapterNumbers = duplicatedChapterNumbers.Distinct().ToList();
                Log.Information($"Found {duplicatedChapterNumbers} duplicated in {manga.Name}");
                foreach(var duplicatedNumber in duplicatedChapterNumbers)
                {
                    var chapters = manga.Chapters.Where(c => c.Name.ToString() == duplicatedNumber.ToString()).OrderBy(c => c.CreatedAt).ToList();
                    for(int i = 1; i < chapters.Count; i++)
                    {
                        await _mangaRepo.Delete(chapters[i].Id);
                    }
                    Log.Information($"Deleted {chapters.Count - 1} {duplicatedNumber} duplicated in {manga.Name}");
                }
            }
        }

    }
}
