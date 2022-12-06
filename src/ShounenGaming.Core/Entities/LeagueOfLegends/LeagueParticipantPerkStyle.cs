using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueParticipantPerkStyle : SimpleEntity
    {
        public int Style { get; set; }
        public string Description { get; set; }
        public List<LeagueParticipantPerkStyleSelection> Selections { get; set; }
    }
}
