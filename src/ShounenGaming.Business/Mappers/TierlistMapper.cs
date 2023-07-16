using AutoMapper;
using ShounenGaming.Core.Entities.Tierlists;
using ShounenGaming.DTOs.Models.Tierlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Mappers
{
    public class TierlistMapper : Profile
    {
        public TierlistMapper()
        {
            CreateMap<Tierlist, TierlistDTO>();
            CreateMap<Tierlist, SimpleTierlistDTO>();
            CreateMap<TierlistItem, TierlistItemDTO>();
            CreateMap<Tier, TierDTO>();
            CreateMap<UserTierlist, UserTierlistDTO>();
            CreateMap<TierlistCategory, TierlistCategoryDTO>();
            CreateMap<TierChoice, TierChoiceDTO>();
        }
    }
}
