using AutoMapper;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        }
    }
}
