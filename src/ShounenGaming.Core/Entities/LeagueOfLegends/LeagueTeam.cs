using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueTeam : SimpleEntity
    {
        public int TeamId { get; set; }
        public bool Win { get; set; }
        public virtual LeagueTeamObjectives Objectives { get; set; }
        public virtual List<LeagueTeamBan> Bans { get; set; }
    }
}
