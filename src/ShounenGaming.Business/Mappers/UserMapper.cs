using AutoMapper;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Core.Entities.Base.Enums;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.DTOs.Models.Base.Enums;
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
            CreateMap<RolesEnum, RolesEnumDTO>();
            CreateMap<ServerMember, ServerMemberDTO>();
            CreateMap<User, UserDTO>()
                .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dto => dto.DiscordImage, opt =>
                {
                    opt.AllowNull();
                    opt.PreCondition(u => u.IsInServer);
                    opt.MapFrom(s => s.ServerMember.ImageUrl);
                })
                .ForMember(dto => dto.Role, opt =>
                {
                    opt.AllowNull();
                    opt.PreCondition(u => u.IsInServer);
                    opt.MapFrom(s => s.ServerMember.Role);
                })
                .ForMember(dto => dto.DiscordId, opt =>
                {
                    opt.AllowNull();
                    opt.PreCondition(u => u.IsInServer);
                    opt.MapFrom(s => s.ServerMember.DiscordId);
                });

            CreateMap<User, SimpleUserDTO>()
                .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dto => dto.DiscordImage, opt =>
                {
                    opt.AllowNull();
                    opt.PreCondition(u => u.IsInServer);
                    opt.MapFrom(s => s.ServerMember.ImageUrl);
                });
        }
    }
}
