using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueParticipantPerksStats : SimpleEntity
    {
        public int Defense { get; set; }
        public int Flex { get; set; }
        public int Offense { get; set; }
    }
}
