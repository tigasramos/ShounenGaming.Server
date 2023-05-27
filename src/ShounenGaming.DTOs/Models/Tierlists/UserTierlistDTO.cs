using ShounenGaming.DTOs.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Tierlists
{
    public class UserTierlistDTO
    {
        public string Name { get; set; }
        public SimpleTierlistDTO Tierlist { get; set; }
        public SimpleUserDTO User { get; set; }
        public List<TierChoiceDTO> Choices { get; set; }
    }
}
