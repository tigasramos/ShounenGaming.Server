using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueParticipantPerks : SimpleEntity
    {
        public virtual LeagueParticipantPerksStats StatPerks { get; set; }
        public virtual List<LeagueParticipantPerkStyle> Styles { get; set; }
    }
}
