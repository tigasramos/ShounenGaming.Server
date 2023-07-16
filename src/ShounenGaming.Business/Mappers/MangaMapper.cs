using AutoMapper;
using Microsoft.Extensions.Configuration;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DTOs.Models;
using ShounenGaming.DTOs.Models.Mangas;
using ShounenGaming.DTOs.Models.Mangas.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Mappers
{
    public class MangaMapper : Profile
    {
        public MangaMapper()
        {
            CreateMap<MangaUserStatusEnum, MangaUserStatusEnumDTO>();
            CreateMap<MangaUserStatusEnumDTO, MangaUserStatusEnum>();
            CreateMap<MangaTypeEnumDTO, MangaTypeEnum>();
            CreateMap<MangaTypeEnum, MangaTypeEnumDTO>();
            CreateMap<MangaTranslationEnumDTO, TranslationLanguageEnum>();
            CreateMap<TranslationLanguageEnum, MangaTranslationEnumDTO>();


            CreateMap<Manga, MangaDTO>()
                .ForMember(m => m.AlternativeNames, (x) => x.MapFrom((a) => a.AlternativeNames.Select(s => new Pair<string, string>() { Id = s.Language, Value = s.Name})))
                .ForMember(m => m.Tags, (x) => x.MapFrom((a) => a.Tags.Select(s => s.Name).ToList()))
                .ForMember(m => m.Chapters, (x) => x.MapFrom((a) => a.Chapters.OrderByDescending(o => o.Name).ToList()))
                .ForMember(m => m.Synonyms, (x) => x.MapFrom((a) => a.Synonyms.Select(s => s.Name).ToList()))
                .ForMember(m => m.AverageScore, (x) => x.MapFrom((a) => ((a.ALScore ?? a.MALScore) + (a.MALScore ?? a.ALScore)) / 2))
                .ForMember(m => m.ImagesUrls, (x) => x.MapFrom((a) => a.ImagesUrls));
            //.ForMember(m => m.ImageUrl, (x) => x.MapFrom((a) => "https://localhost:7252/" + GetThumbnailImageName(a.Name)));

            CreateMap<Manga, MangaInfoDTO>()
                .ForMember(m => m.Tags, (x) => x.MapFrom((a) => a.Tags.Select(s => s.Name).ToList()))
                .ForMember(m => m.ChaptersCount, (x) => x.MapFrom((a) => a.Chapters.Count))
                .ForMember(m => m.MyAnimeListId, (x) => x.MapFrom((a) => a.MangaMyAnimeListID))
                .ForMember(m => m.AnilistId, (x) => x.MapFrom((a) => a.MangaAniListID))
                .ForMember(m => m.LastChapterDate, (x) => {
                    x.AllowNull();
                    x.PreCondition(a => a.Chapters.Any());
                    x.MapFrom((a) => a.Chapters.OrderByDescending(c => c.CreatedAt).First().CreatedAt);
                } )
                .ForMember(m => m.ImagesUrls, (x) => x.MapFrom((a) => a.ImagesUrls));
                //.ForMember(m => m.ImageUrl, (x) => x.MapFrom((a) => "https://localhost:7252/" + GetThumbnailImageName(a.Name)));

            CreateMap<MangaChapter, MangaChapterDTO>()
                .ForMember(m => m.Translations, (x) => x.MapFrom((a) => a.Translations.Where(t => t.IsWorking).OrderByDescending(t => t.CreatedAt)));
            CreateMap<MangaSource, MangaSourceDTO>();
            CreateMap<MangaWriter, MangaWriterDTO>();
            CreateMap<MangaTranslation, MangaTranslationInfoDTO>()
                .ForMember(m => m.Language, (x) => x.MapFrom((a) => a.Language));


        }


        //TODO: Get URL from Settings File
        private static string GetThumbnailImageName(string name)
        {
            var path = $"mangas/{name.NormalizeStringToDirectory()}/";
            var directory = new DirectoryInfo(path);
            var thumbnailFile = directory.GetFiles().Where(f => f.Name.StartsWith("thumbnail")).First();
            return path + thumbnailFile.Name;
        }
    }
}
