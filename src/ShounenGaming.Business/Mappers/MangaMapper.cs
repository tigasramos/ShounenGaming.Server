using AutoMapper;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.Business.Models.Mangas.Enums;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Mangas;
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
        }
    }
}
