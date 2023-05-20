using AutoMapper;
using ShounenGaming.Business.Helpers;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.Business.Models.Mangas;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
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
            CreateMap<MangaTypeDTOEnum, MangaTypeEnum>();
            CreateMap<MangaTypeEnum, MangaTypeDTOEnum>();


            CreateMap<Manga, MangaDTO>()
                .ForMember(m => m.AlternativeNames, (x) => x.MapFrom((a) => a.AlternativeNames.ToDictionary(s => s.Language, s => s.Name)))
                .ForMember(m => m.Tags, (x) => x.MapFrom((a) => a.Tags.Select(s => s.Name).ToList()))
                .ForMember(m => m.ImageUrl, (x) => x.MapFrom((a) => GetThumbnailImageName(a.Name)));

            CreateMap<Manga, MangaInfoDTO>()
                .ForMember(m => m.Tags, (x) => x.MapFrom((a) => a.Tags.Select(s => s.Name).ToList()))
                .ForMember(m => m.ChaptersCount, (x) => x.MapFrom((a) => a.Chapters.Count))
                .ForMember(m => m.ImageUrl, (x) => x.MapFrom((a) => GetThumbnailImageName(a.Name)));

            CreateMap<MangaChapter, MangaChapterDTO>();
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
