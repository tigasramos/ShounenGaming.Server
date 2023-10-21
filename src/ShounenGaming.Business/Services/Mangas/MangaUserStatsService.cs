using AutoMapper;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using ShounenGaming.DTOs.Models.Mangas.Stats;

namespace ShounenGaming.Business.Services.Mangas
{
    public class MangaUserStatsService : IMangaUserStatsService
    {
        private readonly IMangaUserDataRepository _mangaUserDataRepo;
        private readonly IChangedChapterStateActionRepository _mangaChangedChapterStateActionRepository;

        private readonly IMapper _mapper;

        public MangaUserStatsService(IMangaUserDataRepository mangaUserDataRepo, IChangedChapterStateActionRepository mangaChangedChapterStateActionRepository, IMapper mapper)
        {
            _mangaUserDataRepo = mangaUserDataRepo;
            _mangaChangedChapterStateActionRepository = mangaChangedChapterStateActionRepository;
            _mapper = mapper;
        }

        public async Task<UserMangaMainStatsDTO> GetUserMainStats(int userId)
        {
            var mangaUserData = await _mangaUserDataRepo.GetByUser(userId);
            if (mangaUserData is null)
                throw new EntityNotFoundException("MangaUserData");

            var mangaTypeDic = new Dictionary<MangaTypeEnumDTO, int>();
            var userMangaStatusDic = new Dictionary<MangaUserStatusEnumDTO, int>();
            var mangaTagsDic = new Dictionary<string, int>();
            foreach (var userData in mangaUserData)
            {
                if (userData.Status == MangaUserStatusEnum.IGNORED) continue;

                if (userData.Status == MangaUserStatusEnum.COMPLETED ||
                    userData.Status == MangaUserStatusEnum.READING ||
                    userData.Status == MangaUserStatusEnum.ON_HOLD)
                {

                    var convertedType = _mapper.Map<MangaTypeEnumDTO>(userData.Manga.Type);
                    if (mangaTypeDic.ContainsKey(convertedType))
                        mangaTypeDic[convertedType]++;
                    else
                        mangaTypeDic[convertedType] = 1;

                    foreach (var tag in userData.Manga.Tags)
                    {
                        if (mangaTagsDic.ContainsKey(tag.Name))
                            mangaTagsDic[tag.Name]++;
                        else
                            mangaTagsDic[tag.Name] = 1;
                    }
                }

                var dtoStatus = _mapper.Map<MangaUserStatusEnumDTO>(userData.Status);
                if (userMangaStatusDic.ContainsKey(dtoStatus))
                    userMangaStatusDic[dtoStatus]++;
                else
                    userMangaStatusDic[dtoStatus] = 1;

              
            }

            var chapterHistory = await _mangaChangedChapterStateActionRepository.GetChapterHistoryFromUser(userId);
            if (chapterHistory is null)
                throw new EntityNotFoundException("ChapterHistory");

            var dailyActivityDic = new Dictionary<DateOnly, int>();
            foreach (var ch in chapterHistory.OrderByDescending(c => c.CreatedAt))
            {
                if (dailyActivityDic.ContainsKey(DateOnly.FromDateTime(ch.CreatedAt.Date)))
                    dailyActivityDic[DateOnly.FromDateTime(ch.CreatedAt.Date)]++;
                else dailyActivityDic[DateOnly.FromDateTime(ch.CreatedAt.Date)] = 1;
            }

            return new UserMangaMainStatsDTO
            {
                NumOfChapters = mangaUserData.SelectMany(m => m.ChaptersRead).Count(),
                AverageScore = mangaUserData.Where(m => m.Rating is not null && m.Rating > 0).Select(m => m.Rating).Average() ?? 0,
                NumOfMangas = mangaUserData.Where(m => m.Status != MangaUserStatusEnum.IGNORED).Select(m => m.Manga).DistinctBy(d => d.Id).Count(),
                MangaTypeCounters = mangaTypeDic,
                DailyActivityCounters = dailyActivityDic,
                MangaUserStatusCounters = userMangaStatusDic,
                MangaTagsCounters = mangaTagsDic.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        public async Task<List<UserChapterReadHistoryDTO>> GetUserReadingHistory(int userId)
        {
            var chapterHistory = await _mangaChangedChapterStateActionRepository.GetChapterHistoryFromUser(userId);
            if (chapterHistory is null)
                throw new EntityNotFoundException("ChapterHistory");

            var dto = new List<UserChapterReadHistoryDTO>();
            foreach(var ch in chapterHistory.OrderByDescending(c => c.CreatedAt))
            {
                var last = dto.LastOrDefault();
                if (last is null || 
                    last.Manga.Id != ch.Chapter.Manga.Id || 
                    (ch.CreatedAt - last.ReadAt) > TimeSpan.FromHours(2))
                {
                    dto.Add(new UserChapterReadHistoryDTO
                    {
                        NumOfFirstChapter = ch.Chapter.Name,
                        NumOfLastChapter = ch.Chapter.Name,
                        ReadAt = ch.CreatedAt,
                        Manga = _mapper.Map<MangaInfoDTO>(ch.Chapter.Manga)
                    });
                } 
                else
                {
                    if (ch.Chapter.Name < last.NumOfFirstChapter)
                        last.NumOfFirstChapter = ch.Chapter.Name;
                    else if (ch.Chapter.Name > last.NumOfLastChapter)
                        last.NumOfLastChapter = ch.Chapter.Name;

                    last.ReadAt = ch.CreatedAt;
                }
            }

            return dto;
        }

    }
}
